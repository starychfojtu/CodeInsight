using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using CodeInsight.Github.Queries;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using NodaTime;
using IConnection = Octokit.GraphQL.IConnection;
using Issue = CodeInsight.Domain.Issue.Issue;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Import
{
    public sealed class IssueImporter
    {
        private readonly IIssueStorage issueStorage;
        private readonly IIssueRepository issueRepository;

        public IssueImporter(IIssueStorage issueStorage, IIssueRepository issueRepository)
        {
            this.issueStorage = issueStorage;
            this.issueRepository = issueRepository;
        }

        public async Task<Repository> UpdateIssues(IConnection connection, Repository repository)
        {
            var lastIssues = await issueRepository.GetAllOrderedByLastCommitAt(repository.Id, take: 1);
            var lastIss = lastIssues.SingleOption();
            var cursor = (string)null;

            do
            {
                var page = await GetAllIssuesQuery.Execute(connection, repository, take: 50, cursor: cursor).Execute();
                var updatedOrNewIssues = GetUpdatedOrNewIssues(lastIss, page.Items).ToImmutableList();

                await UpdateOrAdd(updatedOrNewIssues).Execute();

                var allIssuesWereNewOrUpdated = updatedOrNewIssues.Count == page.Items.Count;
                cursor = page.HasNextPage && allIssuesWereNewOrUpdated ? page.EndCursor : null;
            }
            while (cursor != null);

            return repository;
        }

        private IO<Task<Unit>> UpdateOrAdd(IReadOnlyList<Issue> issues)
        {
            var ids = issues.Select(i => i.Id);
            var existingIssues = issueRepository.GetAllOrderedByIds(ids).Result;
            var existingIssueIds = existingIssues.Select(i => i.Id).ToImmutableHashSet();
            var (updatedIss, newIss) = issues.Partition(i => existingIssueIds.Contains(i.Id));

            issueStorage.Add(newIss);
            return issueStorage.Update(updatedIss);
        }

        private static IEnumerable<Issue> GetUpdatedOrNewIssues(
            IOption<Issue> lastUpdatedIss,
            IEnumerable<GetAllIssuesQuery.IssueDto> page)
        {
            return  lastUpdatedIss
                .Map(i =>
                {
                    var minUpdatedAt = i.LastUpdateAt.ToDateTimeOffset();
                    return page.TakeWhile(newI => newI.LastUpdateAt >= minUpdatedAt);
                })
                .GetOrElse(page)
                .Select(Map);
        }


        private static Issue Map(GetAllIssuesQuery.IssueDto i)
        {
            return new Issue(
                id: (uint) i.Id,
                title: NonEmptyString.Create(i.Title).Get(),
                url: NonEmptyString.Create(i.Url).Get(),
                repositoryId: NonEmptyString.Create(i.RepositoryId).Get(),
                closedAt: i.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset),
                createdAt: Instant.FromDateTimeOffset(i.CreatedAt), 
                lastUpdateAt: Instant.FromDateTimeOffset(i.LastUpdateAt),
                commentCount: (uint) i.CommentCount
                );
        }
    }
}

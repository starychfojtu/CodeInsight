using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Domain.Common;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Queries;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit.GraphQL;
using PullRequest = CodeInsight.Domain.PullRequest.PullRequest;

namespace CodeInsight.Github.Import
{
    public sealed class PullRequestImporter
    {
        private readonly IPullRequestStorage pullRequestStorage;
        private readonly IPullRequestRepository pullRequestRepository;
        private readonly IRepositoryStorage repositoryStorage;

        public PullRequestImporter(
            IPullRequestStorage pullRequestStorage,
            IRepositoryStorage repositoryStorage,
            IPullRequestRepository pullRequestRepository)
        {
            this.pullRequestStorage = pullRequestStorage;
            this.repositoryStorage = repositoryStorage;
            this.pullRequestRepository = pullRequestRepository;
        }
        
        public async Task<Repository> UpdatePullRequests(IConnection connection, Repository repository, Func<int, Unit> reportProgress)
        {
            var lastPrs = await pullRequestRepository.GetAllOrderedByUpdated(repository.Id, take: 1);
            var lastPr = lastPrs.SingleOption();
            var cursor = (string) null;
            var step = 1;
            var progress = 0;
            
            do
            {
                var page = await GetAllPullRequestByUpdatedQuery.Execute(connection, repository, take: 50, cursor: cursor).Execute();
                var updatedOrNewPullRequests = GetUpdatedOrNewPullRequests(lastPr, page.Items).ToImmutableList();
                
                // TODO: the code doesn't have to wait for this to finish, but DbContext is not thread safe so it is not easy, refactor to separate transaction.
                await UpdateOrAdd(updatedOrNewPullRequests).Execute();

                var allPrsWereNewOrUpdated = updatedOrNewPullRequests.Count == page.Items.Count;
                cursor = page.HasNextPage && allPrsWereNewOrUpdated ? page.EndCursor : null;

                step++;
                progress = progress + 100 / step;
                reportProgress(progress);
            }
            while (cursor != null);
            
            return repository;
        }

        private IO<Task<Unit>> UpdateOrAdd(IReadOnlyList<PullRequest> pullRequests)
        {
            var ids = pullRequests.Select(pr => pr.Id);
            var existingPrs = pullRequestRepository.GetAllByIds(ids).Result;
            var existingPrIds = existingPrs.Select(pr => pr.Id).ToImmutableHashSet();
            var (updatedPrs, newPrs) = pullRequests.Partition(pr => existingPrIds.Contains(pr.Id));

            pullRequestStorage.Add(newPrs);
            return pullRequestStorage.Update(updatedPrs);
        }

        private static IEnumerable<PullRequest> GetUpdatedOrNewPullRequests(IOption<PullRequest> lastUpdatedPr, IEnumerable<GetAllPullRequestByUpdatedQuery.PullRequestDto> page) => 
            lastUpdatedPr
                .Map(pr =>
                {
                    var minUpdatedAt = pr.UpdatedAt.ToDateTimeOffset();
                    return page.TakeWhile(p => p.UpdatedAt >= minUpdatedAt);
                })
                .GetOrElse(page)
                .Select(Map);

        private static PullRequest Map(GetAllPullRequestByUpdatedQuery.PullRequestDto pr) =>
            new PullRequest(
                id: NonEmptyString.Create(pr.Id).Get(),
                repositoryId: NonEmptyString.Create(pr.RepositoryId).Get(),
                title: NonEmptyString.Create(pr.Title).Get(),
                authorId: new AccountId(pr.AuthorLogin),
                deletions: (uint) pr.Deletions,
                additions: (uint) pr.Additions,
                createdAt: Instant.FromDateTimeOffset(pr.CreatedAt),
                updatedAt: Instant.FromDateTimeOffset(pr.UpdatedAt),
                mergedAt: pr.MergedAt.ToOption().Map(Instant.FromDateTimeOffset),
                closedAt: pr.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset),
                commentCount: (uint) pr.CommentCount
            );
    }
}
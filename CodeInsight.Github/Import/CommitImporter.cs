using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Awaits;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit.GraphQL;
using Commit = CodeInsight.Domain.Commit.Commit;

namespace CodeInsight.Github.Import
{
    public sealed class CommitImporter
    {
        private readonly ICommitStorage commitStorage;
        private readonly ICommitRepository commitRepository;

        public CommitImporter(ICommitStorage commitStorage, ICommitRepository commitRepository)
        {
            this.commitStorage = commitStorage;
            this.commitRepository = commitRepository;
        }

        //TODO: Fix UpdateCommits
        public async Task<Repository> UpdateCommits(IConnection connection, Repository repository)
        {
            var lastPrs = await commitRepository.GetAll();
            var lastPr = lastPrs.SingleOption();
            var cursor = (string)null;
            var page = await GetAllCommitsQuery.Execute(connection, repository, take: 50, cursor: cursor).Execute();
            do
            {
                //var updatedOrNewPullRequests = GetUpdatedOrNewPullRequests(lastPr, page.Items).ToImmutableList();
                //await UpdateOrAdd(updatedOrNewPullRequests).Execute();

                //var allPrsWereNewOrUpdated = updatedOrNewPullRequests.Count == page.Items.Count;
                //cursor = page.HasNextPage && allPrsWereNewOrUpdated ? page.EndCursor : null;
            }
            while (cursor != null);

            return repository;

            //TODO: Update
            var tempList = new List<Commit>
            {
                new Commit(NonEmptyString.Create("1").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.UtcNow), NonEmptyString.Create("CommitMsg").Get()),
                new Commit(NonEmptyString.Create("2").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.UtcNow).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("CommitMsg 2").Get()),
                new Commit(NonEmptyString.Create("3").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.UtcNow).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("CommitMsg 3").Get())
            };
            var newEntries = await GetAllCommits.AwaitCommits((Octokit.Connection) connection, repository);
            commitStorage.Add(tempList);
            //commitStorage.Add(newEntries.Select(Map));
            //commitStorage.Add(GetAllCommits.AwaitCommits((Octokit.Connection) connection, repository).Result.Select(Map));

            return repository;
        }

        private IO<Task<Unit>> UpdateOrAdd(IReadOnlyList<Commit> commits)
        {
            var ids = commits.Select(pr => pr.Id);
            var existingCommits = commitRepository.GetAllByIds(ids).Result;
            var existingCommitIds = existingCommits.Select(pr => pr.Id).ToImmutableHashSet();
            var (updatedCommits, newCommits) = commits.Partition(cm => existingCommitIds.Contains(cm.Id));

            commitStorage.Add(newCommits);
            return commitStorage.Update(updatedCommits);
        }

        //TODO: GetUpdatedOrNewCommits
        /*
        private static IEnumerable<Commit> GetUpdatedOrNewPullRequests(IOption<Commit> lastUpdatedPr, IEnumerable<GetAllCommitsQuery.CommitDto> page) =>
            lastUpdatedPr
                .Map(pr =>
                {
                    var minUpdatedAt = pr.UpdatedAt.ToDateTimeOffset();
                    return page.TakeWhile(p => p.UpdatedAt >= minUpdatedAt);
                })
                .GetOrElse(page)
                .Select(Map);

        
        */
        private static Commit Map(GetAllCommits.CommitDto cm) =>
            new Commit(
                id: NonEmptyString.Create(cm.Id).Get(),
                repositoryId: NonEmptyString.Create(cm.RepositoryId).Get(),
                authorName: NonEmptyString.Create(cm.AuthorName).Get(),
                authorId: NonEmptyString.Create(cm.AuthorId).Get(),
                additions: (uint)cm.Additions,
                deletions: (uint)cm.Deletions,
                committedAt: cm.CommittedAt,
                commitMsg: NonEmptyString.Create(cm.CommitMsg).Get()
            );
    }
}

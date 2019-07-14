using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Queries;
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

        public async Task<Repository> UpdateCommits(IConnection connection, Repository repository)
        {
            //TODO: GetUpdate
            //TODO: GetUpdatedOrNewCommits
            //var lastPrs = await commitRepository.GetAll();
            //var lastPr = lastPrs.SingleOption();
            //var cursor = (string)null;
            //TODO: Deserialize
            //var page = await GetAllCommitsQuery.Execute(connection, repository, take: 50, cursor: cursor).Execute();

            commitStorage.Add(new List<Commit>
            {
                new Commit(NonEmptyString.Create("1").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 4, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow), NonEmptyString.Create("CommitMsg").Get()),
                new Commit(NonEmptyString.Create("2").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 1, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("CommitMsg 2").Get()),
                new Commit(NonEmptyString.Create("3").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("CommitMsg 3").Get())
            });

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
        private static Commit Map(GetAllCommitsQuery.CommitDto cm) =>
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

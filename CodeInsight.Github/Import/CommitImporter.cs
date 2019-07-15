using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Awaits;
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

        //TODO: Remove mock up
        public async Task<Repository> UpdateCommits(Octokit.IConnection connection, Repository repository)
        {
            //TO DO: Use GetUpdate
            //TO DO:  Make and Use GetUpdatedOrNewCommits

            /*
            commitStorage.Add(new List<Commit>
            {
                new Commit(NonEmptyString.Create("1").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 60, 34,
                    Instant.FromDateTimeOffset(DateTime.UtcNow),
                    NonEmptyString.Create("CommitMsg ,").Get()),
                new Commit(NonEmptyString.Create("2").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 50, 24,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("CommitMsg 2").Get()),
                new Commit(NonEmptyString.Create("3").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 10, 45,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("CommitMsg 3").Get()),
                new Commit(NonEmptyString.Create("4").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 15, 50,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(4)),
                    NonEmptyString.Create("CommitMsg 4").Get()),
                new Commit(NonEmptyString.Create("5").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 100, 20,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("CommitMsg 2").Get()),
                new Commit(NonEmptyString.Create("6").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 115, 26,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(5)),
                    NonEmptyString.Create("CommitMsg 6").Get()),
                new Commit(NonEmptyString.Create("7").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 24, 22,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(5)),
                    NonEmptyString.Create("CommitMsg 7").Get()),
                new Commit(NonEmptyString.Create("8").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 75, 20,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(5)),
                    NonEmptyString.Create("CommitMsg 8").Get()),
                new Commit(NonEmptyString.Create("9").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 3").Get(), NonEmptyString.Create("1").Get(), 65, 52,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("CommitMsg 9").Get()),
                new Commit(NonEmptyString.Create("10").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 3").Get(), NonEmptyString.Create("1").Get(), 11, 22,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(4)),
                    NonEmptyString.Create("CommitMsg 10").Get())
            });
            */

            var commits = await GetAllCommits.AwaitCommits(connection, repository);

            //var result = commits.Select(Map);

            commitStorage.Add(commits.Select(Map));

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
        private static IEnumerable<Commit> GetUpdatedOrCommits(IOption<Commit> lastUpdatedCm, IEnumerable<GetAllCommitsQuery.CommitDto> page) =>
            lastUpdatedCm
                .Map(cm =>
                {
                    var minUpdatedAt = cm.CommittedAt.ToDateTimeOffset();
                    return page.TakeWhile(c => c.UpdatedAt >= minUpdatedAt);
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
                committedAt: Instant.FromDateTimeOffset(cm.CommittedAt),
                commitMsg: NonEmptyString.Create(cm.CommitMsg).Get()
            );
    }
}

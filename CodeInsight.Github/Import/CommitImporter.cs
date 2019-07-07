using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Awaits;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using NodaTime;
using Octokit.GraphQL;

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
            //TODO: Update
            var tempList = new List<Commit>
            {
                new Commit(NonEmptyString.Create("1").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.Now), NonEmptyString.Create("Comment").Get()),
                new Commit(NonEmptyString.Create("2").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.Now).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("Comment 2").Get()),
                new Commit(NonEmptyString.Create("3").Get(), NonEmptyString.Create(repository.Id.Value).Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeUtc(DateTime.Now).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("Comment 3").Get())
            };
            var newEntries = await GetAllCommits.AwaitCommits((Octokit.Connection) connection, repository);
            commitStorage.Add(tempList);
            commitStorage.Add(newEntries.Select(Map));
            //commitStorage.Add(GetAllCommits.AwaitCommits((Octokit.Connection) connection, repository).Result.Select(Map));

            return repository;
        }

        //TODO: UpdateOrAdd
        //TODO: GetUpdatedOrNewCommits

        private static Commit Map(GetAllCommits.CommitDto cm) =>
            new Commit(
                id: NonEmptyString.Create(cm.Id).Get(),
                repositoryId: NonEmptyString.Create(cm.RepositoryId).Get(),
                authorName: NonEmptyString.Create(cm.AuthorName).Get(),
                authorId: NonEmptyString.Create(cm.AuthorId).Get(),
                additions: (uint)cm.Additions,
                deletions: (uint)cm.Deletions,
                committedAt: cm.CommittedAt,
                comment: NonEmptyString.Create(cm.Comment).Get()
            );
    }
}

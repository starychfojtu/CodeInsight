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
            commitStorage.Add(GetAllCommits.AwaitCommits((Octokit.Connection) connection, repository).Result.Select(Map));

            return repository;
        }

        //TODO: UpdateOrAdd
        //TODO: GetUpdatedOrNewPullRequests

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

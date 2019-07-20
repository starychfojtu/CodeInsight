using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Awaits;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;
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

        public async Task<Repository> UpdateCommits(Octokit.IConnection connection, Repository repository)
        {
            var commits = await GetAllCommits.AwaitCommits(connection, repository);

            AddNew(commits.Select(Map).ToImmutableList());

            return repository;
        }

        private Unit AddNew(IReadOnlyList<Commit> commits)
        {
            var ids = commits.Select(cm => cm.Id);
            var existingCommits = commitRepository.GetAllByIds(ids).Result;
            var existingCommitIds = existingCommits.Select(cm => cm.Id).ToImmutableHashSet();
            var newEntries = commits.Where(cm => !existingCommitIds.Contains(cm.Id));

            return commitStorage.Add(newEntries);
        }

        private static Commit Map(GetAllCommits.CommitDto cm) =>
            new Commit(
                id: NonEmptyString.Create(cm.Id).Get(),
                repositoryId: NonEmptyString.Create(cm.RepositoryId).Get(),
                authorName: NonEmptyString.Create(cm.AuthorName).Get(),
                additions: (uint)cm.Additions,
                deletions: (uint)cm.Deletions,
                committedAt: Instant.FromDateTimeOffset(cm.CommittedAt),
                commitMsg: NonEmptyString.Create(cm.CommitMsg).Get()
            );
    }
}

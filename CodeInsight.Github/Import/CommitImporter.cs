using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
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
            //TODO: After implementing the communication with github, update accordingly

            return repository;
        }
    }
}

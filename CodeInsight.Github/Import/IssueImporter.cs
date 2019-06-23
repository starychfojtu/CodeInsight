using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using CodeInsight.Domain.Repository;
using Octokit.GraphQL;

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
            //TODO: After implementing the communication with github, update accordingly

            return repository;
        }
    }
}

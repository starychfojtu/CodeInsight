using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using CodeInsight.Domain.Repository;

namespace CodeInsight.Data.Issue
{
    //TODO

    public sealed class IssueRepository : IIssueRepository
    {
        public Task<IEnumerable<Domain.Issue.Issue>> GetAllOrderedByLastCommitAt(RepositoryId repositoryId, uint take)
        {
            throw new System.NotImplementedException();
        }
    }
}
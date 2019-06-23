using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;

namespace CodeInsight.Domain.Issue
{
    public interface IIssueRepository
    {
        Task<IEnumerable<Issue>> GetAllOrderedByLastCommitAt(RepositoryId repositoryId, uint take);
    }
}
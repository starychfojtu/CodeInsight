using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Types;

namespace CodeInsight.Domain.Issue
{
    public interface IIssueRepository
    {
        Task<IEnumerable<Issue>> GetAll();

        Task<IEnumerable<Issue>> GetAllOrderedByIds(IEnumerable<uint> ids);

        Task<IEnumerable<Issue>> GetAllOrderedByLastCommitAt(RepositoryId repositoryId, uint take);

        Task<IEnumerable<Issue>> GetAllOrderedByIssueId(RepositoryId repositoryId, uint take);
    }
}
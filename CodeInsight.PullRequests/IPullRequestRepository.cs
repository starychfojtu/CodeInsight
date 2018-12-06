using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain;

namespace CodeInsight.PullRequests
{
    public interface IPullRequestRepository
    {
        Task<IEnumerable<PullRequest>> GetAll();
    }
}
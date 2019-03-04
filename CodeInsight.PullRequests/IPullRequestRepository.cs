using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public interface IPullRequestRepository
    {
        Task<IEnumerable<PullRequest>> GetAll(Instant minCreatedAt);
    }
}
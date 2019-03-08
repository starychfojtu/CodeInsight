using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public interface IPullRequestRepository
    {
        // TODO: Make this accept repository.
        Task<IEnumerable<PullRequest>> GetAllOpenOrClosedAfter(Instant minClosedAt);
    }
}
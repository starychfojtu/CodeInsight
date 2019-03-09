using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public interface IPullRequestRepository
    {
        Task<IEnumerable<PullRequest>> GetAllIntersecting(RepositoryId repositoryId, Interval interval);
    }
}
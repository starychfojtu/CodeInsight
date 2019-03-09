using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using NodaTime;

namespace CodeInsight.Domain.PullRequest
{
    public interface IPullRequestRepository
    {
        Task<IEnumerable<PullRequest>> GetAllIntersecting(RepositoryId repositoryId, Interval interval);
    }
}
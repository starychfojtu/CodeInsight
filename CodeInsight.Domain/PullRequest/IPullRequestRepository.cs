using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Domain.PullRequest
{
    public interface IPullRequestRepository
    {
        Task<IEnumerable<PullRequest>> GetAllByIds(IEnumerable<NonEmptyString> ids);
        
        Task<IEnumerable<PullRequest>> GetAllIntersecting(RepositoryId repositoryId, Interval interval);
        
        Task<IEnumerable<PullRequest>> GetAllOrderedByUpdated(RepositoryId repositoryId, uint take);
    }
}
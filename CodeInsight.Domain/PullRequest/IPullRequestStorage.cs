using System.Collections.Generic;
using CodeInsight.Domain.Common;
using FuncSharp;

namespace CodeInsight.Domain.PullRequest
{
    public enum PullRequestUpdateError
    {
        SomePullRequestNotFound
    }
    
    public interface IPullRequestStorage : IStorage<PullRequest>
    {
        ITry<Unit, PullRequestUpdateError> Update(IEnumerable<PullRequest> pullRequests);
    }
}
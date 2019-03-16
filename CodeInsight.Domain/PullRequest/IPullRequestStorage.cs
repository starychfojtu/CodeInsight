using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Common;
using FuncSharp;
using Monad;

namespace CodeInsight.Domain.PullRequest
{
    public interface IPullRequestStorage : IStorage<PullRequest>
    {
        IO<Task<Unit>> Update(IEnumerable<PullRequest> pullRequests);
    }
}
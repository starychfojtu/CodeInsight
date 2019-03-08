using System.Collections.Generic;
using CodeInsight.Domain;

namespace CodeInsight.PullRequests
{
    public interface IPullRequestStorage
    {
        void Add(IEnumerable<PullRequest> pullRequests);
    }
}
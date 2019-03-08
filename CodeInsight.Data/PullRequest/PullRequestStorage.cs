using System.Collections.Generic;
using System.Linq;
using CodeInsight.PullRequests;

namespace CodeInsight.Data.PullRequest
{
    public sealed class PullRequestStorage : IPullRequestStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public PullRequestStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Add(IEnumerable<Domain.PullRequest> pullRequests)
        {
            dbContext.AddRange(pullRequests.Select(pr => PullRequest.FromDomain(pr)));
            dbContext.SaveChanges();
        }
    }
}
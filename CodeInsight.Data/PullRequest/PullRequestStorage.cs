using System.Collections.Generic;
using System.Linq;
using CodeInsight.Domain.PullRequest;
using FuncSharp;

namespace CodeInsight.Data.PullRequest
{
    public sealed class PullRequestStorage : IPullRequestStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public PullRequestStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Unit Add(IEnumerable<Domain.PullRequest.PullRequest> pullRequests)
        {
            dbContext.AddRange(pullRequests.Select(pr => PullRequest.FromDomain(pr)));
            dbContext.SaveChanges();
            return Unit.Value;
        }
    }
}
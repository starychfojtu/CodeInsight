using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace CodeInsight.Data.PullRequest
{
    public sealed class PullRequestRepository : IPullRequestRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public PullRequestRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public Task<IEnumerable<Domain.PullRequest>> GetAllOpenOrClosedAfter(Instant minClosedAt)
        {
            var minClosed = minClosedAt.ToDateTimeOffset();
            return dbContext.PullRequests.Where(pr =>
                    pr.MergedAt == null ||
                    pr.ClosedAt == null ||
                    pr.MergedAt.Value >= minClosed ||
                    pr.ClosedAt.Value >= minClosed
                )
                .ToListAsync()
                .Map(prs => prs.Select(pr => PullRequest.ToDomain(pr)));

        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Library;
using CodeInsight.Library.Extensions;
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

        public Task<IEnumerable<Domain.PullRequest.PullRequest>> GetAllIntersecting(RepositoryId repositoryId, Interval interval)
        {
            var start = interval.Start.ToDateTimeOffset();
            var end = interval.End.ToDateTimeOffset();
            return dbContext.PullRequests
                .Where(pr =>
                    pr.CreatedAt <= end &&
                    (pr.MergedAt.Value >= start || pr.ClosedAt.Value >= start)
                )
                .ToListAsync()
                .Map(prs => prs.Select(pr => PullRequest.ToDomain(pr)));
        }
    }
}
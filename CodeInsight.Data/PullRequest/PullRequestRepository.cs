using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
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

        public Task<IEnumerable<Domain.PullRequest.PullRequest>> GetAllByIds(IEnumerable<NonEmptyString> ids)
        {
            var prIds = ids.Select(i => i.Value);
            return dbContext.PullRequests
                .Where(pr => prIds.Contains(pr.Id))
                .ToListAsync()
                .Map(prs => prs.Select(PullRequest.ToDomain));
        }

        public Task<IEnumerable<Domain.PullRequest.PullRequest>> GetAllIntersecting(RepositoryId repositoryId, Interval interval)
        {
            var start = interval.Start.ToDateTimeOffset();
            var end = interval.End.ToDateTimeOffset();
            return dbContext.PullRequests
                .Where(pr =>
                    pr.CreatedAt <= end &&
                    (pr.MergedAt == null || pr.MergedAt >= start) &&
                    (pr.ClosedAt == null || pr.ClosedAt >= start)
                )
                .ToListAsync()
                .Map(prs => prs.Select(PullRequest.ToDomain));
        }

        public Task<IEnumerable<Domain.PullRequest.PullRequest>> GetAllOrderedByUpdated(RepositoryId repositoryId, uint take)
        {
            return dbContext.PullRequests
                .Where(pr => pr.RepositoryId == repositoryId.Value.Value)
                .OrderByDescending(pr => pr.UpdatedAt)
                .Take((int)take)
                .ToListAsync()
                .Map(l => l.Select(PullRequest.ToDomain));
        }
    }
}
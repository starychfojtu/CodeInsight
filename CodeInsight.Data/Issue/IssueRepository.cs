using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CodeInsight.Data.Issue
{
    public sealed class IssueRepository : IIssueRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public IssueRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<IEnumerable<Domain.Issue.Issue>> GetAllOrderedByLastCommitAt(RepositoryId repositoryId, uint take)
        {
            return dbContext.Issues
                .Where(i => i.RepositoryId == repositoryId.Value.Value)
                .OrderByDescending(i => i.LastCommitAt)
                .Take((int) take)
                .ToListAsync()
                .Map(l => l.Select(Issue.ToDomain));
        }

        public Task<IEnumerable<Domain.Issue.Issue>> GetAllOrderedByIssueId(RepositoryId repositoryId, uint take)
        {
            return dbContext.Issues
                .Where(i => i.RepositoryId == repositoryId.Value.Value)
                .OrderByDescending(i => i.Id)
                .Take((int)take)
                .ToListAsync()
                .Map(l => l.Select(Issue.ToDomain));
        }
    }
}
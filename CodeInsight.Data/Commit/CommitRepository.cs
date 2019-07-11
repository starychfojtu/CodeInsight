using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace CodeInsight.Data.Commit
{
    public sealed class CommitRepository : ICommitRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public CommitRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<IEnumerable<Domain.Commit.Commit>> GetAllByAuthors(IEnumerable<NonEmptyString> ids)
        {
            var authIds = ids.Select(i => i.Value);
            return dbContext.Commits
                .Where(c => authIds.Contains(c.AuthorId))
                .ToListAsync()
                .Map(c => c.Select(Commit.ToDomain));
        }

        public Task<IEnumerable<Domain.Commit.Commit>> GetAllByIds(IEnumerable<NonEmptyString> ids)
        {
            var commitIds = ids.Select(i => i.Value);
            return dbContext.Commits
                .Where(cm => commitIds.Contains(cm.Id))
                .ToListAsync()
                .Map(cms => cms.Select(Commit.ToDomain));
        }

        public Task<IEnumerable<Domain.Commit.Commit>> GetAll()
        {
            return dbContext.Commits
                .ToListAsync()
                .Map(c => c.Select(Commit.ToDomain));
        }

        public Task<IEnumerable<Domain.Commit.Commit>> GetAllIntersecting(RepositoryId repositoryId, Interval interval)
        {
            var start = interval.Start.ToDateTimeOffset();
            var end = interval.End.ToDateTimeOffset();
            return dbContext.Commits
                .Where(c => 
                    c.RepositoryId == repositoryId.Value.Value && 
                    c.CommittedAt >= start && 
                    c.CommittedAt <= end)
                .ToListAsync()
                .Map(c => c.Select(Commit.ToDomain));
        }
    }
}
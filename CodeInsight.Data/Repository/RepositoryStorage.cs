using System.Collections.Generic;
using CodeInsight.Domain.Repository;
using FuncSharp;

namespace CodeInsight.Data.Repository
{
    public sealed class RepositoryStorage : IRepositoryStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public RepositoryStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Unit Add(IEnumerable<Domain.Repository.Repository> pullRequests)
        {
            dbContext.AddRange(pullRequests);
            dbContext.SaveChanges();
            return Unit.Value;
        }
    }
}
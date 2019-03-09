using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Microsoft.EntityFrameworkCore;

namespace CodeInsight.Data.Repository
{
    public class RepositoryRepository : IRepositoryRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public RepositoryRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<IOption<Domain.Repository.Repository>> Get(NonEmptyString owner, NonEmptyString name)
        {
            return dbContext
                .Repositories
                .Where(r => r.Name == name.Value && r.Owner == owner.Value)
                .SingleOrDefaultAsync()
                .Map(r => r.ToOption())
                .Map(r => r.Map(Repository.ToDomain));
        }
    }
}
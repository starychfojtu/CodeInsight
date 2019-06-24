using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Library.Extensions;
using FuncSharp;
using Monad;

namespace CodeInsight.Data.Commit
{
    public sealed class CommitStorage : ICommitStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public CommitStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Unit Add(IEnumerable<Domain.Commit.Commit> entities)
        {
            dbContext.AddRange(entities.Select(c => Commit.FromDomain(c)));
            dbContext.SaveChanges();
            return Unit.Value;
        }

        public IO<Task<Unit>> Update(IEnumerable<Domain.Commit.Commit> commits)
        {
            var _ =
                from c in commits
                let newC = Commit.FromDomain(c)
                let oldC = dbContext.Find<Commit>(newC.Id).AsOption().Get()
                select Update(oldC, newC);

            return dbContext.SaveChangesAsync().ToUnit();
        }

        private Unit Update(Commit oldC, Commit newC)
        {
            dbContext.Entry(oldC).CurrentValues.SetValues(newC);
            return Unit.Value;
        }
    }
}
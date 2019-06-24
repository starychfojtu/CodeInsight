using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using CodeInsight.Library.Extensions;
using FuncSharp;
using Monad;

namespace CodeInsight.Data.Issue
{
    public sealed class IssueStorage : IIssueStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public IssueStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Unit Add(IEnumerable<Domain.Issue.Issue> entities)
        {
            dbContext.AddRange(entities.Select(i => Issue.FromDomain(i)));
            dbContext.SaveChanges();
            return Unit.Value;
        }

        public IO<Task<Unit>> Update(IEnumerable<Domain.Issue.Issue> issues)
        {
            var _ =
                from i in issues
                let newI = Issue.FromDomain(i)
                let oldI = dbContext.Find<Issue>(newI.Id).AsOption().Get()
                select Update(oldI, newI);

            return dbContext.SaveChangesAsync().ToUnit();
        }

        private Unit Update(Issue oldI, Issue newI)
        {
            dbContext.Entry(oldI).CurrentValues.SetValues(newI);
            return Unit.Value;
        }
    }
}
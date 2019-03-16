using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library.Extensions;
using FuncSharp;
using Monad;

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

        public IO<Task<Unit>> Update(IEnumerable<Domain.PullRequest.PullRequest> pullRequests) => () =>
        {
            var _ =
                from prs in pullRequests
                let newPr = PullRequest.FromDomain(prs)
                let oldPr = dbContext.Find<PullRequest>(newPr.Id).AsOption().Get()
                select Update(oldPr, newPr);

            return dbContext.SaveChangesAsync().ToUnit();
        };

        private Unit Update(PullRequest oldPr, PullRequest newPr)
        {
            dbContext.Entry(oldPr).CurrentValues.SetValues(newPr);
            return Unit.Value;
        }
    }
}
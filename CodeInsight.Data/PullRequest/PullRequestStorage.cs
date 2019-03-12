using System.Collections.Generic;
using System.Linq;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library.Extensions;
using FuncSharp;

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
        
        public ITry<Unit, PullRequestUpdateError> Update(IEnumerable<Domain.PullRequest.PullRequest> pullRequests)
        {
            // TODO: Refactor.
            var results = pullRequests
                .Select(PullRequest.FromDomain)
                .Select(newPr => dbContext
                    .Find<PullRequest>(newPr.Id)
                    .ToOption()
                    .Map(oldPr => Update(oldPr, newPr))
                    .ToTry(_ => PullRequestUpdateError.SomePullRequestNotFound)
                );

            var result = Try.Aggregate(results);
            return result
                .Map(_ => dbContext.SaveChanges().Pipe(rows => Unit.Value))
                .MapError(_ => PullRequestUpdateError.SomePullRequestNotFound);
        }

        private Unit Update(PullRequest oldPr, PullRequest newPr)
        {
            dbContext.Entry(oldPr).CurrentValues.SetValues(newPr);
            return Unit.Value;
        }
    }
}
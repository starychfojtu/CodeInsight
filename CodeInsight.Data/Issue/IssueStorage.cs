using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Issue;
using FuncSharp;
using Monad;

namespace CodeInsight.Data.Issue
{
    //TODO

    public sealed class IssueStorage : IIssueStorage
    {
        public Unit Add(IEnumerable<Domain.Issue.Issue> entities)
        {
            throw new System.NotImplementedException();
        }

        public IO<Task<Unit>> Update(IEnumerable<Domain.Issue.Issue> issues)
        {
            throw new System.NotImplementedException();
        }
    }
}
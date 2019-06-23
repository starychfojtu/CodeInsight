using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Common;
using FuncSharp;
using Monad;

namespace CodeInsight.Domain.Issue
{
    public interface IIssueStorage : IStorage<Issue>
    {
        IO<Task<Unit>> Update(IEnumerable<Issue> issues);
    }
}

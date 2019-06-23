using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Common;
using FuncSharp;
using Monad;

namespace CodeInsight.Domain.Commit
{
    public interface ICommitStorage : IStorage<Commit>
    {
        IO<Task<Unit>> Update(IEnumerable<Commit> commits);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using FuncSharp;
using Microsoft.Extensions.Options;
using Monad;

namespace CodeInsight.Data.Commit
{
    //TODO
    public sealed class CommitStorage : ICommitStorage
    {
        public Unit Add(IEnumerable<Domain.Commit.Commit> entities)
        {
            throw new System.NotImplementedException();
        }

        public IO<Task<Unit>> Update(IEnumerable<Domain.Commit.Commit> commits)
        {
            throw new System.NotImplementedException();
        }
    }
}
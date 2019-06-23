using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Data.Commit
{
    //TODO
    public sealed class CommitRepository : ICommitRepository
    {
        public Task<IEnumerable<Domain.Commit.Commit>> GetAllByAuthor(IEnumerable<NonEmptyString> ids)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Domain.Commit.Commit>> GetAllIntersecting(RepositoryId repositoryId, Interval interval)
        {
            throw new System.NotImplementedException();
        }
    }
}
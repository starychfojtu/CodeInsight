using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Domain.Commit
{
    public interface ICommitRepository
    {
        Task<IEnumerable<Commit>> GetAllByAuthors(IEnumerable<NonEmptyString> ids);

        Task<IEnumerable<Commit>> GetAllByIds(IEnumerable<NonEmptyString> ids);

        Task<IEnumerable<Commit>> GetAll();

        Task<IEnumerable<Commit>> GetAllIntersecting(RepositoryId repositoryId, Interval interval);
    }
}
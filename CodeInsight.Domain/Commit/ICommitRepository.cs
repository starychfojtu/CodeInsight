using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Library.Types;

namespace CodeInsight.Domain.Commit
{
    public interface ICommitRepository
    {
        Task<IEnumerable<Commit>> GetAllByIds(IEnumerable<NonEmptyString> ids);

        Task<IEnumerable<Commit>> GetAll();
    }
}
using System.Threading.Tasks;
using CodeInsight.Library.Types;
using FuncSharp;

namespace CodeInsight.Domain.Repository
{
    public interface IRepositoryRepository
    {
        Task<IOption<Repository>> Get(NonEmptyString owner, NonEmptyString name);
    }
}
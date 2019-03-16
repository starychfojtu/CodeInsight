using System.Threading.Tasks;
using FuncSharp;
using Monad;

namespace CodeInsight.Jobs
{
    public interface IJobExecutionStorage
    {
        Unit Add<T>(JobExecution<T> execution);

        IO<Task<Unit>> Update<T>(JobExecution<T> execution);
    }
}
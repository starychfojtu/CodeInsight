using System;
using System.Threading.Tasks;
using FuncSharp;
using Monad;

namespace CodeInsight.Jobs
{
    public interface IJobExecutionRepository
    {
        IO<Task<IOption<JobExecution<T>>>> Get<T>(Guid id);
    }
}
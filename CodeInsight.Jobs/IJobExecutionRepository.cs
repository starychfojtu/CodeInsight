using System;
using FuncSharp;

namespace CodeInsight.Jobs
{
    public interface IJobExecutionRepository
    {
        IOption<JobExecution<T>> Get<T>(Guid id);
    }
}
using FuncSharp;

namespace CodeInsight.Jobs
{
    public enum JobExecutionUpdateError
    {
        JobExecutionNotFound
    }
    
    public interface IJobExecutionStorage
    {
        Unit Add<T>(JobExecution<T> execution);

        ITry<Unit, JobExecutionUpdateError> Update<T>(JobExecution<T> execution);
    }
}
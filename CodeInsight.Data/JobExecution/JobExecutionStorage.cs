using System.Threading.Tasks;
using CodeInsight.Jobs;
using CodeInsight.Library.Extensions;
using FuncSharp;
using Monad;

namespace CodeInsight.Data.JobExecution
{
    public class JobExecutionStorage : IJobExecutionStorage
    {
        private readonly CodeInsightDbContext dbContext;

        public JobExecutionStorage(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public Unit Add<T>(JobExecution<T> execution)
        {
            dbContext.Add(JobExecution.FromDomain(execution));
            dbContext.SaveChanges();
            return Unit.Value;
        }

        public IO<Task<Unit>> Update<T>(JobExecution<T> execution)
        {
            var newExecution = JobExecution.FromDomain(execution);
            var oldExecution = dbContext.Find<JobExecution>(execution.Id).AsOption().Get();
            return Update(oldExecution, newExecution);
        }

        private IO<Task<Unit>> Update(JobExecution oldExecution, JobExecution newExecution) => () =>
        {
            dbContext.Entry(oldExecution).CurrentValues.SetValues(newExecution);
            return dbContext.SaveChangesAsync().ToUnit();
        };
    }
}
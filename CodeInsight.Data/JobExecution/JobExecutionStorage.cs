using System.Threading.Tasks;
using CodeInsight.Jobs;
using CodeInsight.Library.Extensions;
using FuncSharp;

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

        public ITry<Unit, JobExecutionUpdateError> Update<T>(JobExecution<T> execution)
        {
            var newExecution = JobExecution.FromDomain(execution);
            return dbContext
                .Find<JobExecution>(execution.Id)
                .ToOption()
                .ToTry(_ => JobExecutionUpdateError.JobExecutionNotFound)
                .Map(e => Update(e, newExecution));
        }

        private Unit Update(JobExecution oldExecution, JobExecution newExecution)
        {
            dbContext.Entry(oldExecution).CurrentValues.SetValues(newExecution);
            dbContext.SaveChanges();
            return Unit.Value;
        }
    }
}
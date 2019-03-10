using System;
using CodeInsight.Jobs;
using FuncSharp;

namespace CodeInsight.Data.JobExecution
{
    public sealed class JobExecutionRepository : IJobExecutionRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public JobExecutionRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public IOption<JobExecution<T>> Get<T>(Guid id)
        {
            return dbContext.JobExecutions.Find(id).ToOption().Map(JobExecution.ToDomain<T>);
        }
    }
}
using System;
using System.Threading.Tasks;
using CodeInsight.Jobs;
using CodeInsight.Library.Extensions;
using FuncSharp;
using Monad;

namespace CodeInsight.Data.JobExecution
{
    public sealed class JobExecutionRepository : IJobExecutionRepository
    {
        private readonly CodeInsightDbContext dbContext;

        public JobExecutionRepository(CodeInsightDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public IO<Task<IOption<JobExecution<T>>>> Get<T>(Guid id)
        {
            return () => dbContext.JobExecutions
                .FindAsync(id)
                .Map(e => e
                    .AsOption()
                    .Map(JobExecution.ToDomain<T>)
                );
        }
    }
}
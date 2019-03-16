using System;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Import;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Hangfire;
using Monad;
using Octokit.GraphQL;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Jobs.Instances
{
    public class ImporterJob
    {
        private readonly Importer importer;
        private readonly IJobExecutionStorage storage;
        private readonly IJobExecutionRepository jobExecutionRepository;

        public ImporterJob(Importer importer, IJobExecutionStorage storage, IJobExecutionRepository jobExecutionRepository)
        {
            this.importer = importer;
            this.storage = storage;
            this.jobExecutionRepository = jobExecutionRepository;
        }

        public IO<JobExecution<string>> StartNew(string connectionToken, string applicationName, string name, string owner) => () =>
        {
            var jobExecution = JobExecution<string>.CreateNew();
            storage.Add(jobExecution);
            
            BackgroundJob.Enqueue<ImporterJob>(j => j.Execute(jobExecution.Id, connectionToken, applicationName, name, owner));

            return jobExecution;
        };
        
        public Task Execute(Guid executionId, string connectionToken, string applicationName, string name, string owner)
        {
            var connection = new Connection(new ProductHeaderValue(applicationName), connectionToken);
            var repoOwner = NonEmptyString.Create(owner).Get();
            var repoName = NonEmptyString.Create(name).Get();

            // TODO: Refactor to LINQ.
            var result = importer.ImportRepository(connection, repoOwner, repoName)
                .SelectMany(
                    repository => GetExecution(executionId),
                    (repository, execution) => execution.With(progress: 100, result: Some(repository.Id.Value.Value))
                )
                .Bind(e => storage.Update(e));

            return result.Execute();
        }

        private IO<Task<JobExecution<string>>> GetExecution(Guid executionId)
        {
            return jobExecutionRepository.Get<string>(executionId).Map(e => e.Get());
        }
    }
}
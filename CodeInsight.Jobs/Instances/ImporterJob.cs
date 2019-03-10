using System;
using System.Threading.Tasks;
using CodeInsight.Github.Import;
using CodeInsight.Library.Types;
using FuncSharp;
using Hangfire;
using Octokit.GraphQL;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Jobs.Instances
{
    public class ImporterJob
    {
        private readonly Importer importer;
        private readonly IJobExecutionStorage storage;
        private readonly IJobExecutionRepository repository;

        public ImporterJob(Importer importer, IJobExecutionStorage storage, IJobExecutionRepository repository)
        {
            this.importer = importer;
            this.storage = storage;
            this.repository = repository;
        }

        public JobExecution<string> StartNew(string connectionToken, string applicationName, string name, string owner)
        {
            var jobExecution = JobExecution<string>.CreateNew();
            storage.Add(jobExecution);
            
            BackgroundJob.Enqueue<ImporterJob>(j => j.Execute(jobExecution.Id, connectionToken, applicationName, name, owner));

            return jobExecution;
        }
        
        public void Execute(Guid executionId, string connectionToken, string applicationName, string name, string owner)
        {
                
            var connection = new Connection(new ProductHeaderValue(applicationName), connectionToken);
            var repoOwner = NonEmptyString.Create(owner).Get();
            var repoName = NonEmptyString.Create(name).Get();
            var importedRepository = importer.ImportRepository(connection, repoOwner, repoName).Result;

            var execution = repository.Get<string>(executionId).Get();
            var finishedJobExecution = execution.With(progress: 100, result: Some(importedRepository.Id.Value.Value));
            storage.Update(finishedJobExecution);
        }
    }
}
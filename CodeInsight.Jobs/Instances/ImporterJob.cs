using System;
using System.Threading.Tasks;
using CodeInsight.Github.Import;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using Hangfire;
using Monad;
using Octokit;
using Octokit.Internal;
using static CodeInsight.Library.Prelude;
using Connection = Octokit.GraphQL.Connection;
using ProductHeaderValue = Octokit.GraphQL.ProductHeaderValue;

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
            //TODO: Test
            var connection = new Connection(new ProductHeaderValue(applicationName), connectionToken);
            var conn = new Octokit.Connection(new Octokit.ProductHeaderValue(applicationName), new InMemoryCredentialStore(new Credentials(connectionToken)));
            var repoOwner = NonEmptyString.Create(owner).Get();
            var repoName = NonEmptyString.Create(name).Get();
            
            var result = GetExecution(executionId)
                .SelectMany(
                    _ => importer.ImportRepository(conn, connection, repoOwner, repoName),
                    (e, r) => e.With(progress: 100, result: Some(r.Id.Value.Value))
                )
                .Bind(storage.Update);
            
            return result.Execute();
        }
        
        private IO<Task<JobExecution<string>>> GetExecution(Guid executionId)
        {
            return jobExecutionRepository.Get<string>(executionId).Map(e => e.Get());
        }
    }
}
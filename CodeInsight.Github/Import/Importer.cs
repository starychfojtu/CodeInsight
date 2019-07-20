using System.Threading.Tasks;
using CodeInsight.Domain.Common;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Queries;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using Monad;
using Octokit.GraphQL;

namespace CodeInsight.Github.Import
{
    public sealed class Importer
    {
        private readonly CommitImporter commitImporter;
        private readonly IssueImporter issueImporter;
        private readonly PullRequestImporter pullRequestImporter;
        private readonly IRepositoryStorage repositoryStorage;
        private readonly IRepositoryRepository repositoryRepository;

        public Importer(
            CommitImporter commitImporter, 
            IssueImporter issueImporter, 
            PullRequestImporter pullRequestImporter, 
            IRepositoryStorage repositoryStorage, 
            IRepositoryRepository repositoryRepository)
        {
            this.commitImporter = commitImporter;
            this.issueImporter = issueImporter;
            this.pullRequestImporter = pullRequestImporter;
            this.repositoryStorage = repositoryStorage;
            this.repositoryRepository = repositoryRepository;
        }

        public IO<Task<Repository>> ImportRepository(
            Octokit.IConnection conn,
            IConnection connection, 
            NonEmptyString owner, 
            NonEmptyString name) => () =>

        {
            return GetOrCreateRepository(connection, owner, name)
                .Bind(r => pullRequestImporter.UpdatePullRequests(connection, r))
                .Bind(r => commitImporter.UpdateCommits(conn, r))//;
                .Bind(r => issueImporter.UpdateIssues(connection, r));
        };

        
        private Task<Repository> GetOrCreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) => 
            repositoryRepository.Get(owner, name)
                .Bind(repository => repository.Match(
                    r => r.Async(),
                    _ => CreateRepository(connection, owner, name).Map(AddRepository)
                ));

        private static async Task<Repository> CreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name)
        {
            // TODO - Handle case if repository not found.
            var repositoryDto = (await GetRepositoryQuery.Execute(connection, owner, name).Execute()).Get();
            return new Repository(
                id: new RepositoryId(NonEmptyString.Create(repositoryDto.Id).Get()),
                name: NonEmptyString.Create(repositoryDto.Name).Get(),
                owner: NonEmptyString.Create(repositoryDto.Owner).Get()
            );
        }

        private Repository AddRepository(Repository repository) =>
            repositoryStorage.Add(repository).Pipe(_ => repository);
    }
}
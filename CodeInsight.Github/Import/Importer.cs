using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Domain.Common;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Github.Queries;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit.GraphQL;
using PullRequest = CodeInsight.Domain.PullRequest.PullRequest;

namespace CodeInsight.Github.Import
{
    public sealed class Importer
    {
        private readonly PullRequestImporter pullRequestImporter;
        private readonly IRepositoryStorage repositoryStorage;
        private readonly IRepositoryRepository repositoryRepository;


        public Importer(PullRequestImporter pullRequestImporter, IRepositoryStorage repositoryStorage, IRepositoryRepository repositoryRepository)
        {
            this.pullRequestImporter = pullRequestImporter;
            this.repositoryStorage = repositoryStorage;
            this.repositoryRepository = repositoryRepository;
        }

        public IO<Task<Repository>> ImportRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) => () =>
            GetOrCreateRepository(connection, owner, name)
                .Bind(r => pullRequestImporter.UpdatePullRequests(connection, r, i => Unit.Value));
        
        private Task<Repository> GetOrCreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) => 
            repositoryRepository.Get(owner, name)
                .Bind(repository => repository.Match(
                    r => r.Async(),
                    _ => CreateRepository(connection, owner, name).Map(AddRepository)
                ));

        private static async Task<Repository> CreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name)
        {
            // TODO: Handle case if repository not found.
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
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
using NodaTime;
using Octokit.GraphQL;
using PullRequest = CodeInsight.Domain.PullRequest.PullRequest;

namespace CodeInsight.Github.Import
{
    public sealed class Importer
    {
        private readonly IPullRequestStorage pullRequestStorage;
        private readonly IPullRequestRepository pullRequestRepository;
        private readonly IRepositoryStorage repositoryStorage;
        private readonly IRepositoryRepository repositoryRepository;

        public Importer(
            IPullRequestStorage pullRequestStorage,
            IRepositoryStorage repositoryStorage,
            IRepositoryRepository repositoryRepository,
            IPullRequestRepository pullRequestRepository)
        {
            this.pullRequestStorage = pullRequestStorage;
            this.repositoryStorage = repositoryStorage;
            this.repositoryRepository = repositoryRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        public Task<Repository> ImportRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) =>
            GetOrCreateRepository(connection, owner, name)
                .Bind(r => ImportPullRequests(connection, r));
        
        private Task<Repository> GetOrCreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) => 
            repositoryRepository.Get(owner, name)
                .Bind(repository => repository.Match(
                    r => r.Async(),
                    _ => CreateRepository(connection, owner, name).Map(AddRepository)
                ));

        private static async Task<Repository> CreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name)
        {
            // TODO: Handle case if repository not found.
            var repositoryDto = await GetRepositoryQuery.Get(owner, name).Execute(connection);
            return new Repository(
                id: new RepositoryId(NonEmptyString.Create(repositoryDto.Id).Get()),
                name: NonEmptyString.Create(repositoryDto.Name).Get(),
                owner: NonEmptyString.Create(repositoryDto.Owner).Get()
            );
        }

        private Repository AddRepository(Repository repository) =>
            repositoryStorage.Add(repository).Pipe(_ => repository);

        private async Task<Repository> ImportPullRequests(IConnection connection, Repository repository)
        {
            var lastPrs = await pullRequestRepository.GetAllOrderedByCreated(repository.Id, 1);
            var lastPr = lastPrs.SingleOption();
            var cursor = (string) null;
            
            do
            {
                var query = GetAllPullRequestQuery.Get(repository, take: 50, cursor: cursor);
                var page = await query.Execute(connection);
                var pullRequestsToAdd = GetPullRequestsToAdd(lastPr, page.Items).ToImmutableList();
                
                pullRequestStorage.Add(pullRequestsToAdd);

                var allPrsWereAdded = pullRequestsToAdd.Count == page.Items.Count;
                cursor = page.HasNextPage && allPrsWereAdded ? page.EndCursor : null;
            }
            while (cursor != null);
            
            return repository;
        }

        private static IEnumerable<PullRequest> GetPullRequestsToAdd(IOption<PullRequest> lastPr, IEnumerable<GetAllPullRequestQuery.PullRequestDto> page)
        {
            return lastPr.Match(
                pr =>
                {
                    var minCreatedAt = pr.CreatedAt.ToDateTimeOffset();
                    return page
                        .TakeWhile(p => p.Id != pr.Id.Value && p.CreatedAt > minCreatedAt)
                        .Select(p => Map(p));
                },
                _ => page.Select(Map)
            );
        }

        private static PullRequest Map(GetAllPullRequestQuery.PullRequestDto pr) =>
            new PullRequest(
                id: NonEmptyString.Create(pr.Id).Get(),
                repositoryId: NonEmptyString.Create(pr.RepositoryId).Get(),
                title: NonEmptyString.Create(pr.Title).Get(),
                authorId: new AccountId(pr.AuthorLogin),
                deletions: (uint) pr.Deletions,
                additions: (uint) pr.Additions,
                createdAt: Instant.FromDateTimeOffset(pr.CreatedAt),
                updatedAt: Instant.FromDateTimeOffset(pr.UpdatedAt),
                mergedAt: pr.MergedAt.ToOption().Map(Instant.FromDateTimeOffset),
                closedAt: pr.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset),
                commentCount: (uint) pr.CommentCount
            );
    }
}
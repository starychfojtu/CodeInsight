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
                .Bind(r => UpdatePullRequests(connection, r));
        
        private Task<Repository> GetOrCreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name) => 
            repositoryRepository.Get(owner, name)
                .Bind(repository => repository.Match(
                    r => r.Async(),
                    _ => CreateRepository(connection, owner, name).Map(AddRepository)
                ));

        private static async Task<Repository> CreateRepository(IConnection connection, NonEmptyString owner, NonEmptyString name)
        {
            // TODO: Handle case if repository not found.
            var repositoryDto = (await GetRepositoryQuery.Get(owner, name).Execute(connection)).Get();
            return new Repository(
                id: new RepositoryId(NonEmptyString.Create(repositoryDto.Id).Get()),
                name: NonEmptyString.Create(repositoryDto.Name).Get(),
                owner: NonEmptyString.Create(repositoryDto.Owner).Get()
            );
        }

        private Repository AddRepository(Repository repository) =>
            repositoryStorage.Add(repository).Pipe(_ => repository);

        private async Task<Repository> UpdatePullRequests(IConnection connection, Repository repository)
        {
            var lastPrs = await pullRequestRepository.GetAllOrderedByUpdated(repository.Id, 1);
            var lastPr = lastPrs.SingleOption();
            var cursor = (string) null;
            
            do
            {
                var query = GetAllPullRequestByUpdatedQuery.Get(repository, take: 50, cursor: cursor);
                var page = await query.Execute(connection);
                var updatedOrNewPullRequests = GetUpdatedOrNewPullRequests(lastPr, page.Items).ToImmutableList();
                
                // TODO: the code doesn't have to wait for this to finish, but BbContext is nto thread safe, refactor.
                await UpdateOrAdd(updatedOrNewPullRequests);

                var allPrsWereNewOrUpdated = updatedOrNewPullRequests.Count == page.Items.Count;
                cursor = page.HasNextPage && allPrsWereNewOrUpdated ? page.EndCursor : null;
            }
            while (cursor != null);
            
            return repository;
        }

        private async Task<Unit> UpdateOrAdd(IReadOnlyList<PullRequest> pullRequests)
        {
            var ids = pullRequests.Select(pr => pr.Id);
            var existingPrs = await pullRequestRepository.GetAllByIds(ids);
            var existingPrIds = existingPrs.Select(pr => pr.Id).ToImmutableHashSet();
            var (updatedPrs, newPrs) = pullRequests.Partition(pr => existingPrIds.Contains(pr.Id));

            pullRequestStorage.Add(newPrs);
            return pullRequestStorage.Update(updatedPrs).Success.Get();
        }

        private static IEnumerable<PullRequest> GetUpdatedOrNewPullRequests(IOption<PullRequest> lastUpdatedPr, IEnumerable<GetAllPullRequestByUpdatedQuery.PullRequestDto> page) => 
            lastUpdatedPr
                .Map(pr =>
                {
                    var minUpdatedAt = pr.UpdatedAt.ToDateTimeOffset();
                    return page.TakeWhile(p => p.UpdatedAt >= minUpdatedAt);
                })
                .GetOrElse(page)
                .Select(Map);

        private static PullRequest Map(GetAllPullRequestByUpdatedQuery.PullRequestDto pr) =>
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
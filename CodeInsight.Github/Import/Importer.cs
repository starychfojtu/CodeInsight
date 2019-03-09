using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;
using Octokit.GraphQL;
using PullRequest = CodeInsight.Domain.PullRequest.PullRequest;

namespace CodeInsight.Github.Import
{
    internal sealed class Importer
    {
        private readonly IPullRequestStorage pullRequestStorage;

        public Importer(IPullRequestStorage pullRequestStorage)
        {
            this.pullRequestStorage = pullRequestStorage;
        }

        public Task<Unit> ImportRepository(IConnection connection, string owner, string name)
        {
            // Check if already exists.
            // Add Repository.
            // Add PullRequests.
            return ImportPullRequests(connection, null);
        }

        private async Task<Unit> ImportPullRequests(IConnection connection, Repository repository)
        {
            var cursor = (string) null;
            
            do
            {
                var query = Queries.GetAllPullRequests(repository, take: 50, cursor: cursor);
                var page = await query.Execute(connection);
                var prs = page.Items.Select(Map);
                
                pullRequestStorage.Add(prs);
                
                cursor = page.HasNextPage ? page.EndCursor : null;
            }
            while (cursor != null);
            
            return Unit.Value;
        }
    
        private static PullRequest Map(Queries.PullRequestDto pr) =>
            new PullRequest(
                id: NonEmptyString.Create(pr.Number.ToString()).Get(),
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
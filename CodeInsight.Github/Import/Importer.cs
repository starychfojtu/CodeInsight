using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using FuncSharp;
using NodaTime;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using PullRequest = CodeInsight.Domain.PullRequest;
using static Octokit.GraphQL.Variable;

namespace CodeInsight.Github.Import
{
    public sealed class Importer
    {
        private static ICompiledQuery<ResponsePage<PullRequestDto>> GetAllPullRequestsQuery { get; }

        static Importer()
        {
            GetAllPullRequestsQuery = CreateGetAllPullRequestsQuery();
        }
        
        private readonly GithubRepositoryClient client;
        
        private readonly IPullRequestStorage pullRequestStorage;

        public Importer(GithubRepositoryClient client)
        {
            this.client = client;
        }

        public Task<Unit> ImportRepository(string owner, string name)
        {
            // Check if already exists.
            // Add Repository.
            // Add PullRequests.
            return ImportPullRequests(owner, name);
        }

        private Task<Unit> ImportPullRequests(string owner, string name)
        {
            return ForAllPullRequestPages(owner, name, pullRequestStorage.Add);
        }
        
        private async Task<Unit> ForAllPullRequestPages(string owner, string name, Action<IEnumerable<PullRequest>> action)
        {
            var vars = new Dictionary<string, object>
            {
                { "repositoryName", name },
                { "repositoryOwner", owner },
                { "after", null },
                { "first", 50 }
            };
            
            do
            {
                var page = await client.Connection.Run(GetAllPullRequestsQuery, vars);
                var prs = page.Items.Select(Map);
                
                action(prs);
                
                vars["after"] = page.HasNextPage ? page.EndCursor : null;
            }
            while (vars["after"] != null);
            
            return Unit.Value;
        }
        
        private static ICompiledQuery<ResponsePage<PullRequestDto>> CreateGetAllPullRequestsQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .PullRequests(
                    first: Var("first"),
                    after: Var("after"),
                    orderBy: new IssueOrder
                    {
                        Field = IssueOrderField.CreatedAt,
                        Direction = OrderDirection.Desc
                    }
                )
                .Select(prs => new ResponsePage<PullRequestDto>(
                    prs.PageInfo.HasNextPage,
                    prs.PageInfo.EndCursor,
                    prs.Nodes.Select(pr => new PullRequestDto
                    {
                        RepositoryId = pr.Repository.Id.Value,
                        Number = pr.Number,
                        Title = pr.Title,
                        AuthorLogin = pr.Author.Login,
                        Deletions = pr.Deletions,
                        Additions = pr.Additions,
                        UpdatedAt = pr.UpdatedAt,
                        CreatedAt = pr.CreatedAt,
                        MergedAt = pr.MergedAt,
                        ClosedAt = pr.ClosedAt,
                        CommentCount = pr.Comments(null, null, null, null).TotalCount
                    })
                    .ToList()
                ))
                .Compile();
    
        private static PullRequest Map(PullRequestDto pr) =>
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

        private sealed class PullRequestDto
        {
            public string RepositoryId { get; set; }
            public int Number { get; set; }
            public string Title { get; set; }
            public string AuthorLogin { get; set; }
            public int Deletions { get; set; }
            public int Additions { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public DateTimeOffset? MergedAt { get; set; }
            public DateTimeOffset? ClosedAt { get; set; }
            public int CommentCount { get; set; }
        }
    }
}
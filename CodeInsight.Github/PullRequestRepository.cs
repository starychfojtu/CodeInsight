using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using FuncSharp;
using Newtonsoft.Json;
using NodaTime;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using PullRequest = CodeInsight.Domain.PullRequest;
using static Octokit.GraphQL.Variable;

namespace CodeInsight.Github
{
    public sealed class PullRequestRepository: IPullRequestRepository
    {
        private static ICompiledQuery<ResponsePage<PullRequestDto>> GetAllClosedAfterQuery { get; }
        private static ICompiledQuery<IEnumerable<PullRequestDto>> GetAllOpenQuery { get; }

        static PullRequestRepository()
        {
            GetAllClosedAfterQuery = CreateGetAllClosedAfterQuery();
            GetAllOpenQuery = CreateGetAllOpenQuery();
        }
        
        private readonly GithubRepositoryClient client;

        public PullRequestRepository(GithubRepositoryClient client)
        {
            this.client = client;
        }
        
        public Task<IEnumerable<PullRequest>> GetAllOpenOrClosedAfter(Instant minClosedAt)
        {
            var closedPrs = GetAllClosedAfter(minClosedAt);
            var openPrs = client
                .Connection
                .Run(GetAllOpenQuery, new Dictionary<string, object>
                {
                    { "repositoryName", client.RepositoryName },
                    { "repositoryOwner", client.RepositoryOwner }
                })
                .Map(prs => prs.Select(Map));

            return Task.WhenAll(openPrs, closedPrs).Map(prs => prs.SelectMany(i => i));
        }

        private async Task<IEnumerable<PullRequest>> GetAllClosedAfter(Instant minClosedAt)
        {
            var items = new List<PullRequest>();
            var fetchNextPage = true;
            var vars = new Dictionary<string, object>
            {
                { "repositoryName", client.RepositoryName },
                { "repositoryOwner", client.RepositoryOwner },
                { "after", null },
                { "first", 50 }
            };
            
            do
            {
                var page = await client.Connection.Run(GetAllClosedAfterQuery, vars);
                var prs = page.Items.Select(Map);
                items.AddRange(prs);

                var prWithMinUpdatedAt = page.Items.LastOption();
                var prsMinUpdatedAt = prWithMinUpdatedAt.Map(pr => Instant.FromDateTimeOffset(pr.UpdatedAt));
                fetchNextPage = prsMinUpdatedAt.Map(updatedAt => updatedAt >= minClosedAt).GetOrElse(false);
                
                vars["after"] = page.HasNextPage ? page.EndCursor : null;
            }
            while (vars["after"] != null && fetchNextPage);

            return items;
        }

        private static ICompiledQuery<ResponsePage<PullRequestDto>> CreateGetAllClosedAfterQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .PullRequests(
                    first: Var("first"),
                    after: Var("after"),
                    orderBy: new IssueOrder
                    {
                        Field = IssueOrderField.UpdatedAt,
                        Direction = OrderDirection.Desc
                    },
                    states: new [] { PullRequestState.Merged, PullRequestState.Closed }
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
        
        private static ICompiledQuery<IEnumerable<PullRequestDto>> CreateGetAllOpenQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .PullRequests(states: new [] { PullRequestState.Open })
                .AllPages()
                .Select(pr => new PullRequestDto
                {
                    RepositoryId = pr.Repository.Id.Value,
                    Number = pr.Number,
                    Title = pr.Title,
                    AuthorLogin = pr.Author.Login,
                    Deletions = pr.Deletions,
                    Additions = pr.Additions,
                    CreatedAt = pr.CreatedAt,
                    UpdatedAt = pr.UpdatedAt,
                    MergedAt = pr.MergedAt,
                    ClosedAt = pr.ClosedAt,
                    CommentCount = pr.Comments(null, null, null, null).TotalCount
                })
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
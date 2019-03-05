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
using PullRequest = CodeInsight.PullRequests.PullRequest;
using static Octokit.GraphQL.Variable;

namespace CodeInsight.Github
{
    public sealed class PullRequestRepository: IPullRequestRepository
    {
        private static ICompiledQuery<ResponsePage<PullRequestDto>> GetAllQuery { get; }

        static PullRequestRepository()
        {
            GetAllQuery = CreateGetAllQuery();
        }
        
        private readonly GithubRepositoryClient client;

        public PullRequestRepository(GithubRepositoryClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<PullRequest>> GetAll(Instant minCreatedAt)
        {
            var connection = client.Connection;
            var query = GetAllQuery;
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
                var page = await connection.Run(query, vars);
                var prs = page.Items.Select(Map);
                items.AddRange(prs);

                var prWithMinCreatedAt = page.Items.LastOption();
                var prsMinCreatedAt = prWithMinCreatedAt.Map(pr => Instant.FromDateTimeOffset(pr.CreatedAt));
                fetchNextPage = prsMinCreatedAt.Map(createdAt => createdAt >= minCreatedAt).GetOrElse(false);
                
                vars["after"] = page.HasNextPage ? page.EndCursor : null;
            }
            while (vars["after"] != null && fetchNextPage);

            return items;
        }

        private static ICompiledQuery<ResponsePage<PullRequestDto>> CreateGetAllQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .PullRequests(first: Var("first"), after: Var("after"), orderBy: new IssueOrder
                {
                    Field = IssueOrderField.CreatedAt,
                    Direction = OrderDirection.Desc
                })
                .Select(prs => new ResponsePage<PullRequestDto>(
                    prs.PageInfo.HasNextPage,
                    prs.PageInfo.EndCursor,
                    prs.Nodes.Select(pr => new PullRequestDto
                    {
                        Number = pr.Number,
                        AuthorLogin = pr.Author.Login,
                        Deletions = pr.Deletions,
                        Additions = pr.Additions,
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
                NonEmptyString.Create(pr.Number.ToString()).Get(),
                new AccountId(pr.AuthorLogin),
                (uint) pr.Deletions,
                (uint) pr.Additions,
                Instant.FromDateTimeOffset(pr.CreatedAt),
                pr.MergedAt.ToOption().Map(Instant.FromDateTimeOffset),
                pr.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset),
                (uint) pr.CommentCount
            );

        private sealed class PullRequestDto
        {
            public int Number { get; set; }
            public string AuthorLogin { get; set; }
            public int Deletions { get; set; }
            public int Additions { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? MergedAt { get; set; }
            public DateTimeOffset? ClosedAt { get; set; }
            public int CommentCount { get; set; }
        }
    }
}
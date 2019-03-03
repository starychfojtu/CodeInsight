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
using PullRequest = CodeInsight.PullRequests.PullRequest;

namespace CodeInsight.Github
{
    public sealed class PullRequestRepository: IPullRequestRepository
    {
        private readonly GithubRepositoryClient client;

        public PullRequestRepository(GithubRepositoryClient client)
        {
            this.client = client;
        }

        public Task<IEnumerable<PullRequest>> GetAll() =>
            client.Connection
                .Run(GetPullRequestQuery(client.RepositoryName))
                .Map(prs => prs.Select(Map));

        private static ICompiledQuery<IEnumerable<PullRequestDto>> GetPullRequestQuery(string repositoryName) =>
            new Query()
                .Viewer
                .Repository(repositoryName)
                .PullRequests()
                .Nodes
                .Select(pr => new PullRequestDto
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monad;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github
{
    internal static class GetAllPullRequestByUpdatedQuery
    {
        private static ICompiledQuery<ResponsePage<PullRequestDto>> Query { get; }
        
        static GetAllPullRequestByUpdatedQuery()
        {
            Query = CreateQuery();
        }

        internal static IO<Task<ResponsePage<PullRequestDto>>> Execute(IConnection conn, Repository repository, int take, string cursor = null) => () =>
        {
            var vars = new Dictionary<string, object>
            {
                {"repositoryName", repository.Name.Value},
                {"repositoryOwner", repository.Owner.Value},
                {"after", cursor},
                {"first", take}
            };
            return conn.Run(Query, vars);
        };
        
        private static ICompiledQuery<ResponsePage<PullRequestDto>> CreateQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .PullRequests(
                    first: Var("first"),
                    after: Var("after"),
                    orderBy: new IssueOrder
                    {
                        Field = IssueOrderField.UpdatedAt,
                        Direction = OrderDirection.Desc
                    }
                )
                .Select(prs => new ResponsePage<PullRequestDto>(
                    prs.PageInfo.HasNextPage,
                    prs.PageInfo.EndCursor,
                    prs.Nodes.Select(pr => new PullRequestDto
                        {
                            RepositoryId = pr.Repository.Id.Value,
                            Id = pr.Id.Value,
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
        
        
        internal sealed class PullRequestDto
        {
            public string RepositoryId { get; set; }
            public string Id { get; set; }
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
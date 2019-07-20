using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monad;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Queries
{
    public static class GetAllIssuesQuery
    {
        private static ICompiledQuery<ResponsePage<IssueDto>> Query { get; }

        static GetAllIssuesQuery()
        {
            Query = CreateQuery();
        }

        internal static IO<Task<ResponsePage<IssueDto>>> Execute(
            IConnection conn, 
            Repository repository, 
            int take, 
            string cursor = null) => () =>
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

        private static ICompiledQuery<ResponsePage<IssueDto>> CreateQuery()
        {
            return new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .Issues(
                    first: Var("first"),
                    after: Var("after"),
                    orderBy: new IssueOrder
                    {
                        Field = IssueOrderField.UpdatedAt,
                        Direction = OrderDirection.Desc
                    }
                )
                .Select(issues => new ResponsePage<IssueDto>(
                    issues.PageInfo.HasNextPage,
                    issues.PageInfo.EndCursor,
                    issues.Nodes.Select(issue => new IssueDto
                        {
                            Id = issue.Number,
                            Title = issue.Title,
                            RepositoryId = issue.Repository.Id.Value,
                            ClosedAt = issue.ClosedAt,
                            CreatedAt = issue.CreatedAt,
                            LastUpdateAt = issue.UpdatedAt,
                            CommentCount = issue.Comments(null, null, null, null).TotalCount
                        })
                        .ToList()
                ))
                .Compile();
        }

        internal sealed class IssueDto
        {
            public int Id { get; set; }

            public string Title { get; set; }

            public string RepositoryId { get; set; }
            
            public DateTimeOffset? ClosedAt { get; set; }

            public DateTimeOffset CreatedAt { get; set; }

            public DateTimeOffset LastUpdateAt { get; set; }

            public int CommentCount { get; set; }
        }
    }
}
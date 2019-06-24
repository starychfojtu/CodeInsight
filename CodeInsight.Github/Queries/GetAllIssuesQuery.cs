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
        //TODO: GetAllIssuesQuery - correctly add additions, deletions etc
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
                .Repository("name", "owner")
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
                            Id = issue.Id.Value,
                            RepositoryId = issue.Repository.Id.Value,
                            Additions = 0,
                            Deletions = 0,
                            LastCommitAt = new DateTimeOffset(1998, 02, 17, 7, 00, 00, new TimeSpan(0, 0, 0, 0)),
                            LastUpdateAt = issue.UpdatedAt,
                            ChangedFilesCount = 0,
                            AuthorsCount = 0
                        })
                        .ToList()
                ))
                .Compile();
        }

        internal sealed class IssueDto
        {
            public string Id { get; set; }

            public string RepositoryId { get; set; }

            public int Additions { get; set; }

            public int Deletions { get; set; }

            public DateTimeOffset LastCommitAt { get; set; }

            public DateTimeOffset LastUpdateAt { get; set; }

            public int ChangedFilesCount { get; set; }

            public int AuthorsCount { get; set; }
        }
    }
}
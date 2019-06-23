using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monad;
using Octokit.GraphQL;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Queries
{
    //TODO: Add a way of getting the data from github
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
                .Repository("name", "owner")
                .Issues(null, null, null, null, null, null, null)
                .Select(issues => new ResponsePage<IssueDto>(
                    issues.PageInfo.HasNextPage,
                    issues.PageInfo.EndCursor,
                    issues.Nodes.Select(issue => new IssueDto
                        {
                            Id = issue.Id.Value,
                            RepositoryId = issue.Repository.Id.Value,
                            //TODO: Foreach commit associated with this issue - add to additions etc.
                            Additions = 0,
                            Deletions = 0,
                            LastCommitAt = new DateTimeOffset(1998, 02, 17, 7, 00, 00, new TimeSpan(0, 0, 0, 0)),
                            ChangedFilesCount = 0,
                            AuthorsCount = 0
                        })
                        .ToList()
                ))
                .Compile();
        }
    }

    internal sealed class IssueDto
    {
        public string Id { get; set; }

        public string RepositoryId { get; set; }

        public int Additions { get; set; }

        public int Deletions { get; set; }

        public DateTimeOffset LastCommitAt { get; set; }

        public int ChangedFilesCount { get; set; }

        public int AuthorsCount { get; set; }
    }
}
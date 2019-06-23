using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeInsight.Domain.Repository;
using Monad;
using Octokit;
using Octokit.GraphQL;

namespace CodeInsight.Github.Queries
{
    //TODO: Add a way of getting the data from github
    internal static class GetAllCommitsQuery
    {
        private static ICompiledQuery<ResponsePage<CommitDto>> Query { get; }

        static GetAllCommitsQuery()
        {
            Query = CreateQuery();
        }

        private static ICompiledQuery<ResponsePage<CommitDto>> CreateQuery()
        {
            var github = new GitHubClient(new ProductHeaderValue("MyAmazingApp"));
            var user = await github.User.Get("half-ogre");
            Console.WriteLine(user.Followers + " folks love the half ogre!");
        }

        internal static IO<Task<ResponsePage<CommitDto>>> Execute(IConnection conn, Repository repository, int take, string cursor = null) => () =>
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

        internal sealed class CommitDto
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Monad;
using Octokit;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Queries
{
    //TODO: Add a way of getting the data from github
    internal class GetAllCommits
    {

        private async void AwaitCommits()
        {
            var github = new GitHubClient(new ProductHeaderValue("octokit.net"));
            var commits = await github.Repository.Commit.GetAll(repositoryId: 125);
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

        };

        internal sealed class CommitDto
        {

        }
    }
}

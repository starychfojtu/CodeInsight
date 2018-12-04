using Octokit;
using Repository = CodeInsight.Domain.Repository;

namespace CodeInsight.Github
{
    public class GithubRepositoryClient
    {
        public GithubRepositoryClient(IGitHubClient client, Repository repository)
        {
            Client = client;
            Repository = repository;
        }

        public IGitHubClient Client { get; }
        
        public Repository Repository { get; }
    }
}
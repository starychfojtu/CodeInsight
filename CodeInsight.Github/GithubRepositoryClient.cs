using Octokit;

namespace CodeInsight.Github
{
    public class GithubRepositoryClient
    {
        public GithubRepositoryClient(IGitHubClient api, long repositoryId)
        {
            Api = api;
            RepositoryId = repositoryId;
        }

        public IGitHubClient Api { get; }
        
        public long RepositoryId { get; }
    }
}
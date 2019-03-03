using Octokit;

namespace CodeInsight.Github
{
    public static class Client
    {
        public static IGitHubClient Create(string token, string applicationName)
        {
            return new GitHubClient(new ProductHeaderValue(applicationName))
            {
                Credentials = new Credentials(token)
            };
        }
    }
}
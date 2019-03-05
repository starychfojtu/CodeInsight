using FuncSharp;
using Octokit.GraphQL;

namespace CodeInsight.Github
{
    public class GithubRepositoryClient
    {
        public GithubRepositoryClient(Connection connection, string repositoryName)
        {
            Connection = connection;
            RepositoryName = repositoryName;
        }

        public Connection Connection { get; }
        
        public string RepositoryName { get; }
    }
}
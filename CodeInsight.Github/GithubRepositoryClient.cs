using FuncSharp;
using Octokit.GraphQL;

namespace CodeInsight.Github
{
    public class GithubRepositoryClient
    {
        public GithubRepositoryClient(Connection connection, string repositoryName, string repositoryOwner)
        {
            Connection = connection;
            RepositoryName = repositoryName;
            RepositoryOwner = repositoryOwner;
        }

        public Connection Connection { get; }
        
        public string RepositoryName { get; }
        
        public string RepositoryOwner { get; }
    }
}
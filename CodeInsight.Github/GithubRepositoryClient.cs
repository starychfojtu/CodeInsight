using CodeInsight.Domain.Repository;
using Octokit.GraphQL;

namespace CodeInsight.Github
{
    public class GithubRepositoryClient
    {
        public GithubRepositoryClient(Connection connection, RepositoryId repositoryId)
        {
            Connection = connection;
            RepositoryId = repositoryId;
        }

        public Connection Connection { get; }
        
        public RepositoryId RepositoryId { get; }
    }
}
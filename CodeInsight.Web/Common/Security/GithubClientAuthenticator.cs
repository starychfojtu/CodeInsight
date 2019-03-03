using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Library;
using FuncSharp;
using Octokit;

namespace CodeInsight.Web.Common.Security
{
    public sealed class GithubClientAuthenticator : IClientAuthenticator
    {
        public Task<ITry<Client, AuthenticationError>> Authenticate(SignInParameters parameters)
        {
            return AuthenticateClient(parameters).Map(t => t.Map(c => Client.Github(c)));
        }
        
        private static async Task<ITry<GithubRepositoryClient, AuthenticationError>> AuthenticateClient(SignInParameters parameters)
        {
            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));

            try
            {
                var repository = await client.Repository.Get(parameters.Owner, parameters.Repository);
                var githubClient = new GithubRepositoryClient(client, repository);
                return Try.Success<GithubRepositoryClient, AuthenticationError>(githubClient);
            }
            catch (NotFoundException)
            {
                return Try.Error<GithubRepositoryClient, AuthenticationError>(AuthenticationError.RepositoryNotFound);
            }
        }
    }
}
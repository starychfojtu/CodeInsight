using System.Threading.Tasks;
using CodeInsight.Github;
using FuncSharp;
using Octokit;

namespace CodeInsight.Web.Common.Security
{
    public sealed class GithubClientAuthenticator : IClientAuthenticator
    {
        public async Task<ITry<Client, AuthenticationError>> Authenticate(SignInParameters parameters)
        {
            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));

            try
            {
                var repository = await client.Repository.Get(parameters.Owner, parameters.Repository);
                var githubClient = Client.Github(new GithubRepositoryClient(client, repository));
                return Try.Success<Client, AuthenticationError>(githubClient);
            }
            catch (NotFoundException)
            {
                return Try.Error<Client, AuthenticationError>(AuthenticationError.RepositoryNotFound);
            }
            
        }
    }
}
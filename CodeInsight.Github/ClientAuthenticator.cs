using System.Threading.Tasks;
using FuncSharp;
using Octokit;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Github
{
    public class ClientAuthenticator
    {
        private readonly ApplicationConfiguration configuration;

        public ClientAuthenticator(ApplicationConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IOption<GithubRepositoryClient>> Authenticate(string token, long repositoryId)
        {
            var client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName))
            {
                Credentials = new Credentials(token)
            };

            try
            {
                await client.Authorization.CheckApplicationAuthentication(configuration.ClientId, token);
            }
            catch (NotFoundException)
            {
                return None<GithubRepositoryClient>();
            }

            return Some(new GithubRepositoryClient(client, repositoryId));
        }
    }
}
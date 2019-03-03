using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Library;
using FuncSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Web.Common.Security
{
    public sealed class ClientAuthenticator
    {
        public static readonly string GithubTokenSessionKey = "GithubToken";
        public static readonly string GithubRepositoryIdSessionKey = "GithubRepositoryId";
        
        private readonly Github.ClientAuthenticator githubAuthenticator;
        private readonly IHostingEnvironment environment;

        public ClientAuthenticator(Github.ClientAuthenticator githubAuthenticator, IHostingEnvironment environment)
        {
            this.githubAuthenticator = githubAuthenticator;
            this.environment = environment;
        }

        public async Task<IOption<Client>> Authenticate(HttpContext context)
        {
            var githubClient = await AuthenticateGithubClient(context);
            if (githubClient.NonEmpty)
            {
                return githubClient;
            }
            
            if (environment.IsDevelopment())
            {
                return Some(Client.None());
            }
            
            return None<Client>();
        }

        private Task<IOption<Client>> AuthenticateGithubClient(HttpContext context)
        {
            var githubClientTask =
                from token in context.Session.Get<string>(GithubTokenSessionKey)
                from repositoryId in context.Session.Get<long>(GithubRepositoryIdSessionKey)
                select githubAuthenticator.Authenticate(token, repositoryId);

            return githubClientTask
                .GetOrElse(Task.FromResult(None<GithubRepositoryClient>()))
                .Map(cTask => cTask.Map(c => Client.Github(c)));
        }
    }
}
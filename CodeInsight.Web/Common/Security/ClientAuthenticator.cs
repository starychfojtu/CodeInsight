using CodeInsight.Github;
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

        private readonly ApplicationConfiguration applicationConfiguration;
        private readonly IHostingEnvironment environment;

        public ClientAuthenticator(ApplicationConfiguration applicationConfiguration, IHostingEnvironment environment)
        {
            this.applicationConfiguration = applicationConfiguration;
            this.environment = environment;
        }

        public IOption<Client> Authenticate(HttpContext context)
        {
            var githubClient = AuthenticateGithubClient(context, applicationConfiguration.ApplicationName);
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

        private static IOption<Client> AuthenticateGithubClient(HttpContext context, string applicationName) =>
            from token in context.Session.Get<string>(GithubTokenSessionKey)
            from repositoryId in context.Session.Get<long>(GithubRepositoryIdSessionKey)
            let client = Github.Client.Create(token, applicationName)
            select Client.Github(new GithubRepositoryClient(client, repositoryId));
    }
}
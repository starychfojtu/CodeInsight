using CodeInsight.Domain.Repository;
using CodeInsight.Github;
using CodeInsight.Library.Types;
using FuncSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Octokit.GraphQL;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Web.Common.Security
{
    public sealed class ClientAuthenticator
    {
        public static readonly string GithubTokenSessionKey = "GithubToken";
        public static readonly string GithubRepositoryIdSessionKey = "GithubRepositoryId";

        private readonly ApplicationConfiguration applicationConfiguration;
        private readonly IWebHostEnvironment environment;

        public ClientAuthenticator(ApplicationConfiguration applicationConfiguration, IWebHostEnvironment environment)
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
            from repositoryId in context.Session.Get<string>(GithubRepositoryIdSessionKey)
            let conn = new Connection(new ProductHeaderValue(applicationName), token)
            let repoId = new RepositoryId(NonEmptyString.Create(repositoryId).Get())
            select Client.Github(new GithubRepositoryClient(conn, repoId));
    }
}
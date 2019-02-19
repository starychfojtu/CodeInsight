using CodeInsight.Github;
using CodeInsight.Library;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Octokit;
using Repository = CodeInsight.Domain.Repository;

namespace CodeInsight.Web.Common.Security
{
    public class Authenticator : IAuthenticator
    {
        public Client Authenticate(HttpRequest request)
        {
            var githubClient = new GitHubClient(new ProductHeaderValue("starychfojtu"));
            return GetRepository(request.Cookies)
                .Map(repo => Client.Github(new GithubRepositoryClient(githubClient, repo)))
                .GetOrElse(Client.None());
        }
        
        private static IOption<Repository> GetRepository(IRequestCookieCollection cookies) =>
            from cookieName in cookies.Get("REPO_NAME")
            from name in NonEmptyString.Create(cookieName)
            from cookieOwner in cookies.Get("REPO_OWNER")
            from owner in NonEmptyString.Create(cookieOwner)
            select new Repository(name, owner);
    }
}
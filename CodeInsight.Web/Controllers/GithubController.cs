using System;
using System.Threading.Tasks;
using CodeInsight.Github;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace CodeInsight.Web.Controllers
{
    public class GithubController : Controller
    {
        private readonly ApplicationConfiguration configuration;

        public GithubController(ApplicationConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult GithubSignIn()
        {
            var client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
            
            var csrf = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("GituhCSRF:State", csrf);

            var request = new OauthLoginRequest(configuration.ClientId)
            {
                State = csrf
            };
            var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);

            return Redirect(oauthLoginUrl.ToString());
        }
        
        public async Task<IActionResult> GithubAuthorizeRedirect(string code, string state)
        {
            var client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
            
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var expectedState = HttpContext.Session.GetString("CSRF:State");
            if (state != expectedState)
            {
                return RedirectToAction("Index", "Home");
            }
            
            HttpContext.Session.SetString("CSRF:State", null);

            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            var token = await client.Oauth.CreateAccessToken(request);
            
            HttpContext.Session.SetString("GithubOAuthToken", token.AccessToken);

            return RedirectToAction("Index", "PullRequest");
        }
    }
}
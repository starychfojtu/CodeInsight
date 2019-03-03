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
        private static readonly string OAuthTokenSessionKey = "GithubOAuthToken";
        private static readonly string CsrfSessionKey = "GithubCsrf";
        private static readonly string CurrentRepositoryKey = "GithubCurrentRepository";
        
        private readonly ApplicationConfiguration configuration;
        private readonly GitHubClient client;

        public GithubController(ApplicationConfiguration configuration)
        {
            this.configuration = configuration;
            this.client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
        }

        public IActionResult SignIn()
        {
            var csrf = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(CsrfSessionKey, csrf);

            var request = new OauthLoginRequest(configuration.ClientId)
            {
                State = csrf
            };
            var oauthLoginUrl = client.Oauth.GetGitHubLoginUrl(request);

            return Redirect(oauthLoginUrl.ToString());
        }
        
        public async Task<IActionResult> Authorize(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var expectedState = HttpContext.Session.GetString(CsrfSessionKey);
            if (state != expectedState)
            {
                return RedirectToAction("Index", "Home");
            }
            
            HttpContext.Session.SetString(CsrfSessionKey, null);

            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            var token = await client.Oauth.CreateAccessToken(request);
            
            HttpContext.Session.SetString(OAuthTokenSessionKey, token.AccessToken);

            return RedirectToAction("Index", "PullRequest");
        }
        
        public Task<IActionResult> ChooseRepository()
        {
            throw new NotImplementedException();
        }
    }
}
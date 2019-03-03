using System;
using System.Threading.Tasks;
using CodeInsight.Github;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace CodeInsight.Web.Controllers
{
    public class GithubController : Controller
    {
        private static readonly string CsrfSessionKey = "GithubCsrf";
        
        private readonly ApplicationConfiguration configuration;
        private readonly ClientAuthenticator authenticator;
        private readonly GitHubClient client;

        public GithubController(ApplicationConfiguration configuration, ClientAuthenticator authenticator)
        {
            this.configuration = configuration;
            this.authenticator = authenticator;
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

            var tokenKey = Common.Security.ClientAuthenticator.GithubTokenSessionKey;
            HttpContext.Session.SetString(tokenKey, token.AccessToken);

            return RedirectToAction("Index", "PullRequest");
        }
        
        public Task<IActionResult> ChooseRepository()
        {
            var tokenKey = Common.Security.ClientAuthenticator.GithubTokenSessionKey;
            throw new NotImplementedException();
        }
    }
}
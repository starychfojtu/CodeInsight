using System;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Library;
using CodeInsight.Web.Common;
using CodeInsight.Web.Models.Github;
using FuncSharp;
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
        private readonly GitHubClient client;

        public GithubController(ApplicationConfiguration configuration)
        {
            this.configuration = configuration;
            this.client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
        }
        
        public IActionResult SignIn()
        {
            var csrf = Guid.NewGuid().ToString();
            HttpContext.Session.Set(CsrfSessionKey, csrf);

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

            var expectedState = HttpContext.Session.Get<string>(CsrfSessionKey).GetOrElse("");
            if (state != expectedState)
            {
                return RedirectToAction("Index", "Home");
            }
            
            HttpContext.Session.Remove(CsrfSessionKey);

            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            var token = await client.Oauth.CreateAccessToken(request);

            var tokenKey = Common.Security.ClientAuthenticator.GithubTokenSessionKey;
            HttpContext.Session.Set(tokenKey, token.AccessToken);

            return RedirectToAction("ChooseRepository");
        }
        
        [HttpGet]
        public Task<IActionResult> ChooseRepository()
        {
            var tokenKey = Common.Security.ClientAuthenticator.GithubTokenSessionKey;
            var token = HttpContext.Session.Get<string>(tokenKey);
            return token.Match(
                t =>
                {
                    var apiClient = Client.Create(t, configuration.ApplicationName);
                    return apiClient.Repository
                        .GetAllForCurrent()
                        .Map(rs => rs.Select(r => new RepositoryItem(r.Id, r.FullName)))
                        .Map(items => new ChooseRepositoryViewModel(items))
                        .Map(vm => (IActionResult) View(vm));
                },
                _ => NotFound().Async<NotFoundResult, IActionResult>()
            );
        }
        
        [HttpPost]
        public Task<IActionResult> ChooseRepository(long id)
        {
            var tokenKey = Common.Security.ClientAuthenticator.GithubTokenSessionKey;
            var token = HttpContext.Session.Get<string>(tokenKey);
            return token.Match(
                t =>
                {
                    var apiClient = Client.Create(t, configuration.ApplicationName);
                    var repository = apiClient.Repository.Get(id);
                    return repository.SafeMap(r => r.Match<IActionResult>(
                        s =>
                        {
                            var repoKey = Common.Security.ClientAuthenticator.GithubRepositoryIdSessionKey;
                            HttpContext.Session.Set(repoKey, s.Id);
                            return RedirectToAction("Index", "PullRequest");
                        },
                        e => BadRequest()
                    ));
                },
                _ => NotFound().Async<NotFoundResult, IActionResult>()
            );
        }
    }
}
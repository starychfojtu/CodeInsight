using System;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Library;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Github;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Octokit.GraphQL;
using Client = CodeInsight.Github.Client;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using static CodeInsight.Library.Prelude;
using Connection = Octokit.GraphQL.Connection;
using ProductHeaderValue = Octokit.ProductHeaderValue;

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
            var csrfToken = Guid.NewGuid().ToString();
            var oauthLoginUrl = GetOAuthLoginUrl(csrfToken);
            
            HttpContext.Session.Set(CsrfSessionKey, csrfToken);

            return Redirect(oauthLoginUrl.ToString());
        }

        public Task<IActionResult> Authorize(string code, string state)
        {
            var session = HttpContext.Session;
            var verifiedCode = GetVerifiedCode(code, state, session);
            var oAuthToken = verifiedCode.Map(vc => GetOAuthAccessToken(vc));
            return oAuthToken.Match(
                tokenTask => tokenTask.Map(token =>
                {
                    session.Remove(CsrfSessionKey);
                    session.Set(ClientAuthenticator.GithubTokenSessionKey, token);
                    return (IActionResult)RedirectToAction("ChooseRepository");
                }),
                _ => RedirectToAction("Index", "Home").Async<RedirectToActionResult, IActionResult>()
            );
        }

        [HttpGet]
        public Task<IActionResult> ChooseRepository() => UserClientAction(userClient =>
        {
            return userClient.Repository
                .GetAllForCurrent()
                .Map(rs => rs.Select(r => new RepositoryInputDto(r.Id, r.FullName)))
                .Map(items => new ChooseRepositoryViewModel(items))
                .Map(vm => (IActionResult) View(vm));
        });

        [HttpPost]
        public Task<IActionResult> ChooseRepository(long id) => UserClientAction(async userClient =>
        {
            try
            {
                var repository = await userClient.Repository.Get(id);
                HttpContext.Session.Set(ClientAuthenticator.GithubRepositoryIdSessionKey, repository.Id);
                return RedirectToAction("Index", "PullRequest");
            }
            catch (NotFoundException)
            {
                return BadRequest();
            }
        });
        
        private static IOption<NonEmptyString> GetVerifiedCode(string code, string state, ISession session)
        {
            var expectedState = session.Get<string>(CsrfSessionKey).GetOrElse("");
            return NonEmptyString.Create(code).Where(_ => state == expectedState);
        }

        private Uri GetOAuthLoginUrl(string csrf)
        {
            return client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(configuration.ClientId)
            {
                State = csrf
            });
        }
        
        private Task<string> GetOAuthAccessToken(NonEmptyString code)
        {
            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            return client.Oauth.CreateAccessToken(request).Map(t => t.AccessToken);
        }

        private Task<IActionResult> UserClientAction(Func<IGitHubClient, Task<IActionResult>> action)
        {
            var token = HttpContext.Session.Get<string>(ClientAuthenticator.GithubTokenSessionKey);
            return token.Match(
                t => action(Client.Create(t, configuration.ApplicationName)),
                _ => NotFound().Async<NotFoundResult, IActionResult>()
            );
        }
    }
}
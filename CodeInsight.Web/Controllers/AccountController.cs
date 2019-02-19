using System;
using System.Threading.Tasks;
using CodeInsight.Library;
using CodeInsight.Web.Common.Security;
using FuncSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace CodeInsight.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClientAuthenticator authenticator;

        public AccountController(IClientAuthenticator authenticator)
        {
            this.authenticator = authenticator;
        }
        
        public Task<IActionResult> AnonymousSignIn(string owner, string repository)
        {
            return GetSignInParameters(owner, repository)
                .BindTry(authenticator.Authenticate)
                .Map(client => client.Match(
                    c => FinishSignIn(c),
                    errors => RedirectToAction(nameof(HomeController.Index), "Home"))
                );
        }

        private Task<ITry<SignInParameters>> GetSignInParameters(string owner, string repository)
        {
            var repo = 
                from o in NonEmptyString.Create(owner)
                from r in NonEmptyString.Create(repository)
                select new SignInParameters(o, r);
            
            return repo.ToTry(_ => new ApplicationException("Sign in parameters are invalid.")).Async();
        }

        private IActionResult FinishSignIn(Client client)
        {
            // TODO: For some reason asp.net checks some append policy of cookie, which is true if
            // TODO: cookie is essential or the response cookie class "CanTrack()".
            // TODO: Fix SameSite: lax to strict.

            client.Match(
                github =>
                {
                    var options = new CookieOptions {IsEssential = true};
                    Response.Cookies.Append("REPO_OWNER", github.Repository.Owner.Login, options);
                    Response.Cookies.Append("REPO_NAME",  github.Repository.Name, options);
                },
                none => { }
            );
            
            return RedirectToAction(nameof(PullRequestController.Index), "PullRequest");
        }
    }
}
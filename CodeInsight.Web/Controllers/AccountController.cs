using System.Threading.Tasks;
using CodeInsight.Library;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        private Task<ITry<SignInParameters, AuthenticationError>> GetSignInParameters(string owner, string repository)
        {
            var repo = 
                from o in NonEmptyString.Create(owner)
                from r in NonEmptyString.Create(repository)
                select new SignInParameters(o, r);
            
            return repo.ToTry(_ => AuthenticationError.RepositoryNotFound).Async();
        }

        private IActionResult FinishSignIn(Client client)
        {
            HttpContext.Session.SetClient(client);
            return RedirectToAction(nameof(PullRequestController.Index), "PullRequest");
        }
    }
}
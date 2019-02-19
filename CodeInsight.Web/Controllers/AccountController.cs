using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace CodeInsight.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHostingEnvironment environment;

        public AccountController(IHostingEnvironment environment)
        {
            this.environment = environment;
        }
        
        public async Task<IActionResult> AnonymousSignIn(string owner, string repository)
        {
            if (environment.IsDevelopment())
            {
                return RedirectToAction(nameof(PullRequestController.Index), "PullRequest");
            }
            
            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));

            try
            {
                var repo = await client.Repository.Get(owner, repository);

                // TODO: For some reason asp.net checks some append policy of cookie, which is true if
                // TODO: cookie is essential or the response cookie class "CanTrack()".
                // TODO: Fix SameSite: lax to strict.
                var options = new CookieOptions { IsEssential = true };
                Response.Cookies.Append("REPO_OWNER", repo.Owner.Login, options);
                Response.Cookies.Append("REPO_NAME", repo.Name, options);
                
                return RedirectToAction(nameof(PullRequestController.Index), "PullRequest");
            }
            catch (NotFoundException)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
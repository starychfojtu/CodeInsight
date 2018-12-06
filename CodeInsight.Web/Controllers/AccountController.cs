using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Octokit;

namespace CodeInsight.Web.Controllers
{
    public class AccountController : Controller
    {
        // TODO: Implement real sign in
        public async Task<IActionResult> AnonymousSignIn(string owner, string repository)
        {
            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));

            try
            {
                var repo = await client.Repository.Get(owner, repository);
            
                HttpContext.Response.Cookies.Append("REPO_OWNER", repo.Owner.Login);
                HttpContext.Response.Cookies.Append("REPO_NAME", repo.Name);

                return RedirectToAction(nameof(PullRequestController.Index), "PullRequest");
            }
            catch (NotFoundException e)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
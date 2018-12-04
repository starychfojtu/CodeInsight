using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using static CodeInsight.Web.Common.Authorization;

namespace CodeInsight.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        // TODO: Implement real sign in
        public async Task<IActionResult> AnonymousSignIn(string owner = null, string repositoryName = null)
        {
//            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));
//            var repository = await client.Repository.Get(owner ?? "starychfojtu", repositoryName ?? "SmartRecipes");
            
            // TODO: Check if repository exists;
            
            HttpContext.Response.Cookies.Append("REPO_OWNER", "starychfojtu");
            HttpContext.Response.Cookies.Append("REPO_NAME", "SmartRecipes");

            // TODO: Redirect properly
            return Redirect("/PullRequest/Index");
        }
    }
}
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
        
        public async Task<IActionResult> AnonymousSignIn(string owner, string repositoryName)
        {
            var client = new GitHubClient(new ProductHeaderValue("starychfojtu"));
            var repository = await client.Repository.Get(owner, repositoryName);
            
            // TODO: Check if repository exists;
            
            HttpContext.Response.Cookies.Append("REPO_OWNER", owner);
            HttpContext.Response.Cookies.Append("REPO_NAME", repositoryName);

            // TODO: Redirect properly
            return Redirect("TODO: redirect");
        }
    }
}
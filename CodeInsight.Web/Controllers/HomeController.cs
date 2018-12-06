using Microsoft.AspNetCore.Mvc;

namespace CodeInsight.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
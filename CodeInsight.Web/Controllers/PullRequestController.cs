using System;
using System.Threading.Tasks;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Octokit;
using static CodeInsight.Library.Prelude;
using static CodeInsight.Web.Common.Authentication;

namespace CodeInsight.Web.Controllers
{
    public class PullRequestController : Controller
    {
        private readonly IHostingEnvironment environment;

        public PullRequestController(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        public Task<IActionResult> Index() => 
            PullRequestAction(HttpContext.Request, repository =>
            {
                // TODO: Make this per user
                var zone = DateTimeZone.Utc;
                var today = SystemClock.Instance.GetCurrentInstant().InZone(zone).Date;
                var interval = new DateInterval(
                    today.Minus(Period.FromMonths(12)),
                    today
                );
                
                return repository.GetAll()
                    .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, new ZonedDateInterval(interval, zone)))
                    .Map(s => new PullRequestIndexViewModel(s))
                    .Map(vm => (IActionResult)View(vm));
            });

        private Task<IActionResult> PullRequestAction(HttpRequest request, Func<IPullRequestRepository, Task<IActionResult>> f) =>
            AuthorizedAction(request, environment, client => f(client.Match<IPullRequestRepository>(
                gitHubClient => new Github.PullRequestRepository(gitHubClient),
                none => new SampleRepository()
            )));
    }
}
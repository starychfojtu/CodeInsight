using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using static CodeInsight.Library.Prelude;
using static CodeInsight.Web.Common.Authorization;

namespace CodeInsight.Web.Controllers
{
    public class PullRequestController : Controller
    {
        public IActionResult Index() => 
            AuthorizedAction(HttpContext.Request, client =>
            {
                var start = new LocalDate(2018, 11, 20);
                var createdAt = start.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
                var pr1 = new PullRequest(NonEmptyString.Create("1").Get(), 10, 20, createdAt, Some(createdAt.Plus(Duration.FromDays(5))), None<Instant>());
                var pr2 = new PullRequest(NonEmptyString.Create("2").Get(), 20, 40, createdAt, Some(createdAt.Plus(Duration.FromDays(9))), None<Instant>());
                var pr3 = new PullRequest(NonEmptyString.Create("3").Get(), 30, 60, createdAt, None<Instant>(), None<Instant>());
                var prs = new [] { pr1, pr2, pr3 };
            
    //            var repo = new Repository(NonEmptyString.Create("siroky").Get(), NonEmptyString.Create("FuncSharp").Get());
    //            var nowUtc = DateTime.UtcNow;
    //            var vm = await Github.PullRequests
    //                .Get(repo, new DateTimeInterval(new DateTime(2010, 10, 10), nowUtc))
    //                .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, nowUtc))
    //                .Map(s => new PullRequestIndexViewModel(s))
    //                .Execute(new GitHubClient(new ProductHeaderValue("starychfojtu")));
    
                var interval = new ZonedDateInterval(
                    new DateInterval(
                        start,
                        start.Plus(Period.FromDays(10))
                    ), 
                    DateTimeZone.Utc
                );
                var statistics = RepositoryStatisticsCalculator.Calculate(prs, interval);
                return View(new PullRequestIndexViewModel(statistics));
            });
    }
}
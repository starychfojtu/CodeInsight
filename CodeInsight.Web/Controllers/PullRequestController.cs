using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Models;
using FuncSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Octokit;
using static CodeInsight.Library.Prelude;
using static CodeInsight.Web.Common.Authentication;
using PullRequest = CodeInsight.PullRequests.PullRequest;

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
                var zone = DateTimeZone.Utc;
                var today = SystemClock.Instance.GetCurrentInstant().InZone(zone).Date;
                var interval = new DateInterval(
                    today.Minus(Period.FromMonths(1)),
                    today
                );
                var zonedDateInterval = new ZonedDateInterval(interval, zone);
                
                return repository.GetAll()
                    .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, zonedDateInterval))
                    .Map(statistics => CreateChart(
                        zonedDateInterval.DateInterval,
                        CreateDataSet("Average lifetime", statistics, s => s.AverageLifeTime.TotalHours)
                    ))
                    .Map(chart => new PullRequestIndexViewModel(chart))
                    .Map(vm => (IActionResult)View(vm));
            });

        public Task<IActionResult> PerAuthors() => 
            PullRequestAction(HttpContext.Request, repository =>
            {
                var zone = DateTimeZone.Utc;
                var today = SystemClock.Instance.GetCurrentInstant().InZone(zone).Date;
                var interval = new DateInterval(
                    today.Minus(Period.FromMonths(1)),
                    today
                );
                var zonedInterval = new ZonedDateInterval(interval, zone);
                
                return repository.GetAll()
                    .Map(prs => prs
                        .GroupBy(pr => pr.AuthorId)
                        .ToDictionary(g => g.Key, g => RepositoryStatisticsCalculator.Calculate(g, zonedInterval))
                    )
                    .Map(statistics => CreateChart(
                        zonedInterval.DateInterval,
                        statistics.Select(kvp => CreateDataSet(kvp.Key, kvp.Value, s => s.AverageLifeTime.TotalHours)).ToArray()
                    ))
                    .Map(c => new PullRequestIndexViewModel(c))
                    .Map(vm => (IActionResult)View(vm));
            });
        
        private Dataset CreateDataSet(string label, RepositoryDayStatistics statistics, Func<RepositoryStatistics, double> getValue)
        {
            var dates = statistics.Interval.DateInterval;
            var values = dates.Select(d => statistics.Get(d).Map(getValue).GetOrElse(double.NaN));

            return new LineDataset
            {
                Label = label,
                Data = values.ToImmutableList(),
                Fill = "false"
            };
        }
        
        private Chart CreateChart(DateInterval interval, params Dataset[] dataSets)
        {
            return new Chart
            {
                Type = "line",
                Data = new Data
                {
                    Labels = interval.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                    Datasets = dataSets
                }
            };
        }

        private Task<IActionResult> PullRequestAction(HttpRequest request, Func<IPullRequestRepository, Task<IActionResult>> f) =>
            AuthorizedAction(request, environment, client => f(client.Match<IPullRequestRepository>(
                gitHubClient => new Github.PullRequestRepository(gitHubClient),
                none => new SampleRepository()
            )));
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Models.PullRequests;
using FuncSharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using static CodeInsight.Web.Common.Authentication;
using Chart = CodeInsight.Web.Common.Charts.Chart;

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
                        "Average pull request lifetime",
                        zonedDateInterval.DateInterval,
                        CreateDataSets(
                            statistics, 
                            ("Average", s => s.AverageLifeTime.TotalHours),
                            ("Weighted average by changes", s => s.ChangesWeightedAverageLifeTime.TotalHours)
                        )
                    ))
                    .Map(charts => new ChartsViewModel(charts.ToEnumerable()))
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
                    .Map(statistics => CreatePerAuthorCharts(zonedInterval.DateInterval, statistics))
                    .Map(charts => new ChartsViewModel(charts))
                    .Map(vm => (IActionResult)View(vm));
            });
        
        private static IEnumerable<Chart> CreatePerAuthorCharts(DateInterval interval, IReadOnlyDictionary<AccountId, RepositoryDayStatistics> statistics)
        {
            yield return CreateChart(
                "Pull request average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(kvp.Value, (kvp.Key, s => s.AverageLifeTime.TotalHours))).ToList()
            );
            
            yield return CreateChart(
                "Pull request changes weight average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(kvp.Value, (kvp.Key, s => s.ChangesWeightedAverageLifeTime.TotalHours))).ToList()
            );
        }

        private static ImmutableArray<Dataset> CreateDataSets(
            RepositoryDayStatistics statistics,
            params (string label, Func<RepositoryStatistics, double> getValue)[] dataSetParameters)
        {
            var dataSets = dataSetParameters.Select(p => new Dataset
            {
                Label = p.label,
                Data = new List<double>()
            }).ToArray();
            
            var dates = statistics.Interval.DateInterval;
            foreach (var date in dates)
            {
                var statisticsForDate = statistics.Get(date);
                for (var i = 0; i < dataSets.Length; i++)
                {
                    var newValue = statisticsForDate
                        .Map(dataSetParameters[i].getValue)
                        .GetOrElse(double.NaN);
                    dataSets[i].Data.Add(newValue);
                }
            }

            return dataSets.ToImmutableArray();
        }
        
        private static Chart CreateChart(string title, DateInterval interval, IList<Dataset> dataSets)
        {
            return new Chart(title, ChartType.Line, new Data
            {
                Labels = interval.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                Datasets = dataSets
            });
        }

        private Task<IActionResult> PullRequestAction(HttpRequest request, Func<IPullRequestRepository, Task<IActionResult>> f) =>
            AuthorizedAction(request, environment, client => f(client.Match<IPullRequestRepository>(
                gitHubClient => new Github.PullRequestRepository(gitHubClient),
                none => new SampleRepository()
            )));
    }
}
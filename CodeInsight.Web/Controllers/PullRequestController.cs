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
                
                return repository.GetAll()
                    .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, new ZonedDateInterval(interval, zone)))
                    .Map(statistics => CreateChart(statistics))
                    .Map(chart => new PullRequestIndexViewModel(chart))
                    .Map(vm => (IActionResult)View(vm));
            });

        private Chart CreateChart(RepositoryDayStatistics statistics)
        {
            var dates = statistics.Interval.DateInterval;
            // TODO: use weightedValues for another chart
            var (values, weightedValues) = dates
                .Select(d => statistics.Get(d))
                .BiSelect(
                    d => d.Map(s => s.AverageLifeTime.TotalHours).GetOrElse(double.NaN),
                    d => d.Map(s => s.ChangesWeightedAverageLifeTime.TotalHours).GetOrElse(double.NaN)
                );
            
            return new Chart
            {
                Type = "line",
                Data = new Data
                {
                    Labels = dates.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                    Datasets = new List<Dataset> { 
                        new LineDataset
                        {
                            Label = "Average lifetime",
                            Data = values.ToImmutableList(),
                            Fill = "false"
                        }
                    }
                }
            };
        }

        public Task<IActionResult> PerAuthors() => 
            PullRequestAction(HttpContext.Request, repository =>
            {
                var zone = DateTimeZone.Utc;
                var today = SystemClock.Instance.GetCurrentInstant().InZone(zone).Date;
                var interval = new DateInterval(
                    today.Minus(Period.FromMonths(1)),
                    today
                );
                
                return repository.GetAll()
                    .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, new ZonedDateInterval(interval, zone)))
                    .Map(statistics => CreatePerAuthorChart(statistics))
                    .Map(c => new PullRequestIndexViewModel(c))
                    .Map(vm => (IActionResult)View(vm));
            });

        private Chart CreatePerAuthorChart(RepositoryDayStatistics statistics)
        {
            var dates = statistics.Interval.DateInterval;
            var statisticsByAuthorAndDate = statistics.GetByAuthorsForInterval();
            var authors = statisticsByAuthorAndDate.Domain2.Distinct();
            var statisticsByAuthor = statisticsByAuthorAndDate.SliceDimension1();
            var empty = new DataCube1<AccountId, RepositoryStatistics>();
            var dataSets = authors.ToDataCube(a => a, _ => new List<double>());
    
            foreach (var date in dates)
            {
                var dateStats = statisticsByAuthor.Get(date).GetOrElse(empty);
                dataSets.ForEach((accountId, values) =>
                {
                    var newValue = dateStats.Get(accountId).Map(s => s.AverageLifeTime.TotalHours);
                    values.Add(newValue.GetOrElse(double.NaN));
                });
            }

            var lineDataSets = dataSets.Select((accountId, values) => (Dataset)new LineDataset
            {
                Label = accountId,
                Data = values,
                Fill = "false",
            });

            return new Chart
            {
                Type = "line",
                Data = new Data
                {
                    Labels = dates.Select(d => $"{d.Day}.{d.Month}").ToImmutableList(),
                    Datasets = lineDataSets.ToImmutableList()
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
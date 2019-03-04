using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Chart = CodeInsight.Web.Common.Charts.Chart;

namespace CodeInsight.Web.Controllers
{
    public class PullRequestController : AuthorizedController
    {
        public PullRequestController(ClientAuthenticator clientAuthenticator) : base(clientAuthenticator)
        {
        }
        
        public Task<IActionResult> Index() => PullRequestAction(repository =>
        {
            var zone = DateTimeZone.Utc;
            var now = SystemClock.Instance.GetCurrentInstant();
            var tomorrow = now.InZone(zone).Date.PlusDays(1);
            var interval = new DateInterval(
                tomorrow.Minus(Period.FromMonths(1)),
                tomorrow
            );
            var zonedInterval = new ZonedDateInterval(interval, zone);
            var minCreatedAt = interval.Start.At(LocalTime.Midnight).InUtc().ToInstant();
            var configuration = new RepositoryDayStatisticsConfiguration(zonedInterval, now);
            
            return repository.GetAll(minCreatedAt)
                .Map(prs => RepositoryStatisticsCalculator.Calculate(prs, configuration))
                .Map(statistics => CreateDataSets(
                    statistics,
                    ("Average", s => s.AverageLifeTime.TotalHours),
                    ("Weighted average by changes", s => s.ChangesWeightedAverageLifeTime.Map(t => t.TotalHours).ToNullable())
                ))
                .Map(dataSets => Chart.FromInterval("Average pull request lifetime", zonedInterval.DateInterval, dataSets))
                .Map(charts => new ChartsViewModel(ImmutableList.Create(charts)))
                .Map(vm => (IActionResult)View(vm));
        });

        public Task<IActionResult> PerAuthors() => PullRequestAction(repository =>
        {
            var zone = DateTimeZone.Utc;
            var now = SystemClock.Instance.GetCurrentInstant();
            var tomorrow = now.InZone(zone).Date.PlusDays(1);
            var interval = new DateInterval(
                tomorrow.Minus(Period.FromMonths(1)),
                tomorrow
            );
            var zonedInterval = new ZonedDateInterval(interval, zone);
            var minCreatedAt = interval.Start.At(LocalTime.Midnight).InUtc().ToInstant();
            var configuration = new RepositoryDayStatisticsConfiguration(zonedInterval, now);
            
            return repository.GetAll(minCreatedAt)
                .Map(prs => prs
                    .GroupBy(pr => pr.AuthorId)
                    .ToDictionary(g => g.Key, g => RepositoryStatisticsCalculator.Calculate(g, configuration))
                )
                .Map(statistics => CreatePerAuthorCharts(zonedInterval.DateInterval, statistics))
                .Map(charts => new ChartsViewModel(charts.ToImmutableList()))
                .Map(vm => (IActionResult)View(vm));
        });
        
        private static IEnumerable<Chart> CreatePerAuthorCharts(DateInterval interval, IReadOnlyDictionary<AccountId, RepositoryDayStatistics> statistics)
        {
            yield return Chart.FromInterval(
                "Pull request average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(kvp.Value, (kvp.Key, s => s.AverageLifeTime.TotalHours))).ToList()
            );
            
            yield return Chart.FromInterval(
                "Pull request changes weight average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(
                    kvp.Value, 
                    (kvp.Key, s => s.ChangesWeightedAverageLifeTime.Map(t => t.TotalHours).ToNullable())
                )).ToList()
            );
        }

        private static ImmutableArray<Dataset> CreateDataSets(
            RepositoryDayStatistics statistics,
            params (string label, Func<RepositoryStatistics, double?> getValue)[] dataSetParameters)
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

        private Task<IActionResult> PullRequestAction(Func<IPullRequestRepository, Task<IActionResult>> f) => Action(c =>
        {
            var repository = c.Match<IPullRequestRepository>(
                gitHubClient => new Github.PullRequestRepository(gitHubClient),
                none => new SampleRepository()
            );
            return f(repository);
        });
    }
}
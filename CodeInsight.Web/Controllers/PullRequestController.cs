using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models;
using CodeInsight.Web.Models.PullRequest;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Chart = CodeInsight.Web.Common.Charts.Chart;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Web.Controllers
{
    public class PullRequestController : AuthorizedController
    {
        private static readonly Duration EstimatedAveragePullRequestMaxLifetime = Duration.FromDays(30);
        
        public PullRequestController(ClientAuthenticator clientAuthenticator) : base(clientAuthenticator)
        {
        }
        
        public Task<IActionResult> Index(string fromIso8601) => PullRequestAction(async repository =>
        {
            // TODO: Return error in view when invalid.
            var fromIsValid = DateTimeOffset.TryParse(fromIso8601, out var fromDateTimeOffset);
            var from = fromIsValid ? Some(fromDateTimeOffset) : None<DateTimeOffset>();
            var configuration = CreateConfiguration(from);
            var start = configuration.Interval.Start;
            var minCreatedAt = start.ToInstant().Minus(EstimatedAveragePullRequestMaxLifetime);

            var prs = await repository.GetAllOpenOrClosedAfter(minCreatedAt);
            var pullRequests = prs.ToImmutableList();
            var statistics = RepositoryStatisticsCalculator.Calculate(pullRequests, configuration);
            var dataSets = CreateAverageDataSets(statistics);
            var chart = Chart.FromInterval("Average pull request lifetime", configuration.Interval.DateInterval, dataSets);
            var vm = new PullRequestIndexViewModel(start.ToDateTimeOffset(), pullRequests, ImmutableList.Create(chart));
            return (IActionResult)View(vm);
        });

        public Task<IActionResult> PerAuthors() => PullRequestAction(repository =>
        {
            var configuration = CreateConfiguration(None<DateTimeOffset>());
            var start = configuration.Interval.Start;
            var minCreatedAt = start.ToInstant().Minus(EstimatedAveragePullRequestMaxLifetime);
            
            return repository.GetAllOpenOrClosedAfter(minCreatedAt)
                .Map(prs => prs
                    .GroupBy(pr => pr.AuthorId)
                    .ToDictionary(g => g.Key, g => RepositoryStatisticsCalculator.Calculate(g, configuration))
                )
                .Map(statistics => CreatePerAuthorCharts(configuration.Interval.DateInterval, statistics))
                .Map(charts => new ChartsViewModel(charts.ToImmutableList()))
                .Map(vm => (IActionResult)View(vm));
        });
        
        private static RepositoryDayStatisticsConfiguration CreateConfiguration(IOption<DateTimeOffset> from)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fromZoned = from.Match(
                f =>  ZonedDateTime.FromDateTimeOffset(f),
                _ => now.InUtc().Minus(Duration.FromDays(30))
            );
            var zone = fromZoned.Zone;
            var today = now.InZone(zone).Date;
            var interval = new DateInterval(fromZoned.Date, today);
            var zonedInterval = new ZonedDateInterval(interval, zone);
            
            return new RepositoryDayStatisticsConfiguration(zonedInterval, now);
        }
        
        private static IReadOnlyList<Dataset> CreateAverageDataSets(RepositoryDayStatistics statistics)
        {
            return CreateDataSets(
                statistics,
                new LineDataSetConfiguration(
                    "Average",
                    s => s.AverageLifeTime.TotalHours,
                    Color.LawnGreen
                ),
                new LineDataSetConfiguration(
                    "Weighted average by changes",
                    s => s.ChangesWeightedAverageLifeTime.Map(t => t.TotalHours).ToNullable(),
                    Color.ForestGreen
                )
            );
        }
        
        private static IEnumerable<Chart> CreatePerAuthorCharts(DateInterval interval, IReadOnlyDictionary<AccountId, RepositoryDayStatistics> statistics)
        {
            var colors = statistics.Keys.ToDictionary(id => id, _ => ColorExtensions.CreateRandom());
            
            yield return Chart.FromInterval(
                "Pull request average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(
                    kvp.Value,
                    new LineDataSetConfiguration(
                        kvp.Key, 
                        s => s.AverageLifeTime.TotalHours,
                        colors[kvp.Key]
                    )
                )).ToList()
            );
            
            yield return Chart.FromInterval(
                "Pull request changes weight average lifetimes per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(
                    kvp.Value,
                    new LineDataSetConfiguration(
                        kvp.Key,
                        s => s.ChangesWeightedAverageLifeTime.Map(t => t.TotalHours).ToNullable(),
                        colors[kvp.Key]
                    )
                )).ToList()
            );
        }

        private static IReadOnlyList<Dataset> CreateDataSets(RepositoryDayStatistics statistics, params LineDataSetConfiguration[] lineDataSetConfigurations)
        {
            var dataSets = lineDataSetConfigurations.Select(c =>
            {
                var color = c.Color.ToArgbString();
                var colorList = new List<string> { color };
                return new LineDataset
                {
                    Label = c.Label,
                    Data = new List<double>(),
                    BorderColor = color,
                    BackgroundColor = color,
                    PointBorderColor = colorList,
                    PointHoverBorderColor = colorList,
                    PointBackgroundColor = colorList,
                    PointHoverBackgroundColor = colorList,
                    Fill = "false"
                };
            }).ToArray();
            
            var dates = statistics.Interval.DateInterval;
            foreach (var date in dates)
            {
                var statisticsForDate = statistics.Get(date);
                for (var i = 0; i < dataSets.Length; i++)
                {
                    var newValue = statisticsForDate
                        .Map(lineDataSetConfigurations[i].ValueGetter)
                        .GetOrElse(double.NaN);
                    dataSets[i].Data.Add(newValue);
                }
            }

            return dataSets;
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
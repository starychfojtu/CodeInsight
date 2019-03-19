using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Domain.Repository;
using CodeInsight.Library;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models;
using CodeInsight.Web.Models.PullRequest;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Chart = CodeInsight.Web.Common.Charts.Chart;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Web.Controllers
{
    public class PullRequestController : AuthorizedController
    {
        private readonly IPullRequestRepository pullRequestRepository;

        public PullRequestController(IPullRequestRepository pullRequestRepository, ClientAuthenticator clientAuthenticator) : base(clientAuthenticator)
        {
            this.pullRequestRepository = pullRequestRepository;
        }

        #region Index

        public Task<IActionResult> Index(string fromDate, string toDate) => Action(async client =>
        {
            var cultureInfo = GetCultureInfo(HttpContext.Request);
            // TODO: Return error in view when invalid.
            var fromIsValid = DateTimeOffset.TryParse(fromDate, cultureInfo, DateTimeStyles.AssumeLocal, out var fromDateTimeOffset);
            var from = fromIsValid ? Some(fromDateTimeOffset) : None<DateTimeOffset>();
            var configuration = CreateConfiguration(from);
            var instantInterval = new FiniteInterval(
                configuration.Interval.Start.ToInstant(),
                configuration.Interval.End.ToInstant()
            );

            var prs = await pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, instantInterval);
            var pullRequests = prs.ToImmutableList();
            var statistics = StatisticsCalculator.Calculate(pullRequests, configuration);
            var charts = CreateIndexCharts(statistics);
            var vm = new PullRequestIndexViewModel(configuration, pullRequests, charts.ToList());
            return (IActionResult)View(vm);
        });
        
        private static IEnumerable<Chart> CreateIndexCharts(IntervalStatistics statistics)
        {
            var dateSets = CreateDataSets(
                statistics,
                new LineDataSetConfiguration(
                    "Average lifetime",
                    s => s.AverageLifeTime.TotalHours,
                    Color.LawnGreen
                ),
                new LineDataSetConfiguration(
                    "Efficiency",
                    s => s.AverageEfficiency,
                    Color.ForestGreen
                )
            ).ToImmutableArray();
            
            yield return Chart.FromInterval(
                "Average pull request lifetime",
                statistics.Interval.DateInterval,
                new List<Dataset> { dateSets[0] }
            );
            
            yield return Chart.FromInterval(
                "Average efficiency",
                statistics.Interval.DateInterval,
                new List<Dataset> { dateSets[1] }
            );
        }
        
        #endregion

        #region PerAuthors

        public Task<IActionResult> PerAuthors() => Action(client =>
        {
            var configuration = CreateConfiguration(None<DateTimeOffset>());
            var interval = new FiniteInterval(
                configuration.Interval.Start.ToInstant(),
                configuration.Interval.End.ToInstant()
            );
            
            return pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, interval)
                .Map(prs => prs
                    .GroupBy(pr => pr.AuthorId)
                    .ToDictionary(g => g.Key, g => StatisticsCalculator.Calculate(g, configuration))
                )
                .Map(statistics => CreatePerAuthorCharts(configuration.Interval.DateInterval, statistics))
                .Map(charts => new ChartsViewModel(charts.ToImmutableList()))
                .Map(vm => (IActionResult)View(vm));
        });
        
        private static IEnumerable<Chart> CreatePerAuthorCharts(DateInterval interval, IReadOnlyDictionary<AccountId, IntervalStatistics> statistics)
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
                "Efficiency per author",
                interval,
                statistics.SelectMany(kvp => CreateDataSets(
                    kvp.Value,
                    new LineDataSetConfiguration(
                        kvp.Key,
                        s => s.AverageEfficiency.Value,
                        colors[kvp.Key]
                    )
                )).ToList()
            );
        }

        #endregion

        #region Efficiency

        public Task<IActionResult> Efficiency() => Action(async client =>
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var finiteInterval = new FiniteInterval(
                now.Minus(Duration.FromDays(365)),
                now
            );

            // TODO: Change to average efficiency, not duration.
            var prs = await pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, finiteInterval);
            var statistics = prs
                .Select(pr => pr.Lifetime.Map(l => (
                    Efficiency: Domain.Efficiency.Create(pr.TotalChanges, l),
                    Changes: pr.TotalChanges
                )))
                .Flatten()
                .Where(s => s.Changes <= 1000);
            
            var data = statistics.Select(s => new LineScatterData { x = s.Changes.ToString(), y = s.Efficiency.ToString() }).ToList();
            var chartData = new ChartJSCore.Models.Data
            {
                Datasets = new List<Dataset>
                {
                    new LineScatterDataset
                    {
                        Fill = "false",
                        ShowLine = false,
                        Label = "Efficiency",
                        Data = data
                    }
                }
            };
            
            var chart = new Chart("Efficiency per pull request size", ChartType.Scatter, chartData);
            var vm = new EfficiencyViewModel(ImmutableList.Create(chart));
            return (IActionResult)View(vm);
        });

        #endregion
        
        private static IntervalStatisticsConfiguration CreateConfiguration(IOption<DateTimeOffset> from)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fromZoned = from.Match(
                f =>  ZonedDateTime.FromDateTimeOffset(f),
                _ => now.InUtc().Minus(Duration.FromDays(30))
            );
            var zone = fromZoned.Zone;
            var tomorrow = now.InZone(zone).Date.PlusDays(1);
            var interval = new DateInterval(fromZoned.Date, tomorrow);
            var zonedInterval = new ZonedDateInterval(interval, zone);
            
            return new IntervalStatisticsConfiguration(zonedInterval, now);
        }

        private static IReadOnlyList<Dataset> CreateDataSets(IntervalStatistics statistics, params LineDataSetConfiguration[] lineDataSetConfigurations)
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
    }
}
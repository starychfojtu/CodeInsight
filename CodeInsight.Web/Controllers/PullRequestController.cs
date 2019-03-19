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

        public enum ConfigurationError
        {
            InvalidFromDate,
            InvalidToDate,
            ToDateIsAfterFrom,
            ToDateIsAfterTomorrow
        }

        public Task<IActionResult> Index(string fromDate, string toDate) => Action(async client =>
        {
            var cultureInfo = GetCultureInfo(HttpContext.Request);
            var customConfiguration =
                from start in fromDate.ToOption()
                from end in toDate.ToOption()
                select CreateConfiguration(start, end, cultureInfo, DateTimeZone.Utc);

            var (configuration, error) = customConfiguration.Match(
                custom => custom.Match(
                    c => (c, None<string>()),
                    e => (CreateDefaultConfiguration(cultureInfo, DateTimeZone.Utc), Some(e.Match(
                        ConfigurationError.InvalidFromDate, _ => "Invalid from date.",
                        ConfigurationError.InvalidToDate, _ => "Invalid to date.",
                        ConfigurationError.ToDateIsAfterFrom, _ => "Start cannot be after end.",
                        ConfigurationError.ToDateIsAfterTomorrow, _ => "End cannot be after tomorrow."
                    )))
                ),
                _ => (CreateDefaultConfiguration(cultureInfo, DateTimeZone.Utc), None<string>())
            );
            
            var instantInterval = new FiniteInterval(
                configuration.Interval.Start.ToInstant(),
                configuration.Interval.End.ToInstant()
            );

            var prs = await pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, instantInterval);
            var pullRequests = prs.ToImmutableList();
            var statistics = StatisticsCalculator.Calculate(pullRequests, configuration);
            var charts = CreateIndexCharts(statistics);
            var vm = new PullRequestIndexViewModel(configuration, pullRequests, error, charts.ToList());
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
            var cultureInfo = GetCultureInfo(HttpContext.Request);
            var configuration = CreateDefaultConfiguration(cultureInfo, DateTimeZone.Utc);
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

        private static IntervalStatisticsConfiguration CreateDefaultConfiguration(CultureInfo cultureInfo, DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var end = now.InZone(zone).Date.PlusDays(1);
            var start = end.PlusDays(-30);
            return new IntervalStatisticsConfiguration(new ZonedDateInterval(new DateInterval(start, end), zone), now);
        }
        
        private static ITry<IntervalStatisticsConfiguration, ConfigurationError> CreateConfiguration(
            string fromDate,
            string toDate,
            CultureInfo cultureInfo,
            DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var maxToDate = now.InZone(zone).Date.PlusDays(1);

            var start = ParseDate(fromDate, cultureInfo).ToTry(_ => ConfigurationError.InvalidFromDate);
            var end = ParseDate(toDate, cultureInfo)
                .ToTry(_ => ConfigurationError.InvalidToDate)
                .FlatMap(d => (d <= maxToDate).ToTry(t => d, f => ConfigurationError.ToDateIsAfterTomorrow));

            return
                from e in end
                from s in start
                from interval in CreateInterval(s, e).ToTry(_ => ConfigurationError.ToDateIsAfterFrom)
                select new IntervalStatisticsConfiguration(new ZonedDateInterval(interval, zone), now);
        }

        private static IOption<DateInterval> CreateInterval(LocalDate start, LocalDate end)
        {
            return end < start ? None<DateInterval>() : Some(new DateInterval(start, end));
        }

        private static IOption<LocalDate> ParseDate(string date, CultureInfo cultureInfo)
        {
            DateTimeOffset.TryParse("12/20/2018", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.AssumeLocal, out var res);
            var dateIsValid = DateTimeOffset.TryParse(date, cultureInfo, DateTimeStyles.AssumeLocal, out var result);
            var resultAsOffset = dateIsValid ? Some(result) : None<DateTimeOffset>();
            return resultAsOffset.Map(ZonedDateTime.FromDateTimeOffset).Map(d => d.Date);
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
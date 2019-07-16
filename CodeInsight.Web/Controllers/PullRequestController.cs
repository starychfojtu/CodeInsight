using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library.DatePicker;
using CodeInsight.Library.Types;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.PullRequest;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Chart = CodeInsight.Web.Common.Charts.Chart;
using static CodeInsight.Library.Prelude;
using IntervalStatisticsConfiguration = CodeInsight.Library.DatePicker.IntervalStatisticsConfiguration;

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

        public Task<IActionResult> Index(string fromDate, string toDate) => StatisticsAction(fromDate, toDate, (config, pullRequests) =>
        {
            var statistics = StatisticsCalculator.Calculate(pullRequests, config);
            return CreateIndexCharts(statistics).ToImmutableList();
        });
        
        private static IEnumerable<Chart> CreateIndexCharts(IntervalStatistics statistics)
        {
            var averageLifeTimeData = statistics.Map(s => s.AverageLifeTime.TotalHours);
            var averageLifeTimeConfiguration = new LineDataSetConfiguration("Average lifetime", Color.LawnGreen);
            var averageLifetimeDataSet = CreateDataSet(statistics.Interval.DateInterval, averageLifeTimeData, averageLifeTimeConfiguration);
            
            yield return Chart.FromInterval(
                "Average pull request lifetime",
                statistics.Interval.DateInterval,
                new List<Dataset> { averageLifetimeDataSet },
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Hours").Get()
            );
            
            var efficiencyData = statistics.Map(s => s.AverageEfficiency.Value);
            var efficiencyConfiguration = new LineDataSetConfiguration("Efficiency", Color.ForestGreen);
            var efficiencyDataSet = CreateDataSet(statistics.Interval.DateInterval, efficiencyData, efficiencyConfiguration);
            
            yield return Chart.FromInterval(
                "Average efficiency",
                statistics.Interval.DateInterval,
                new List<Dataset> { efficiencyDataSet },
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Efficiency index").Get()
            );
        }
        
        #endregion

        #region PerAuthors

        public Task<IActionResult> PerAuthors(string fromDate, string toDate) => StatisticsAction(fromDate, toDate, (config, pullRequests) =>
        {
            var statistics = pullRequests
                .GroupBy(pr => pr.AuthorId)
                .ToDictionary(g => g.Key, g => StatisticsCalculator.Calculate(g, config));
            
            return CreatePerAuthorCharts(config.Interval.DateInterval, statistics).ToImmutableList();
        });
        
        private static IEnumerable<Chart> CreatePerAuthorCharts(DateInterval interval, IReadOnlyDictionary<AccountId, IntervalStatistics> statistics)
        {
            var colors = statistics.Keys.ToDictionary(id => id, _ => ColorExtensions.CreateRandom());
            
            yield return Chart.FromInterval(
                "Pull request average lifetimes per author",
                interval,
                statistics.Select(kvp => CreateDataSet(
                    interval,
                    kvp.Value.Map(v => v.AverageLifeTime.TotalHours),
                    new LineDataSetConfiguration(kvp.Key, colors[kvp.Key])
                )).ToList(),
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Hours").Get()
            );
            
            yield return Chart.FromInterval(
                "Efficiency per author",
                interval,
                statistics.Select(kvp => CreateDataSet(
                    interval,
                    kvp.Value.Map(v => v.AverageEfficiency.Value),
                    new LineDataSetConfiguration(kvp.Key, colors[kvp.Key])
                )).ToList(),
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Efficiency index").Get()
            );
        }

        #endregion

        #region Efficiency

        public Task<IActionResult> EfficiencyAndChanges() => Action(async client =>
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var finiteInterval = new FiniteInterval(
                now.Minus(Duration.FromDays(365)),
                now
            );
            
            var prs = await pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, finiteInterval);
            var data = prs
                .Where(pr => pr.TotalChanges <= 1000 && pr.Lifetime.NonEmpty)
                .Select(pr => new LineScatterData {
                    y = Efficiency.Create(pr.TotalChanges, pr.Lifetime.Get()).Value.ToString(),
                    x = pr.TotalChanges.ToString()
                })
                .ToList();
            
            var chart = new Chart(
                "Efficiency per pull request size", 
                ChartType.Scatter, 
                Chart.CreateScatterData("Efficiency", data),
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Efficiency index").Get()
            );
            return View(new EfficiencyViewModel(ImmutableList.Create(chart)));
        });

        #endregion
        
        private delegate IReadOnlyList<Chart> IntervalStatisticsAction(IntervalStatisticsConfiguration config, IEnumerable<PullRequest> prs);

        private Task<IActionResult> StatisticsAction(string fromDate, string toDate, IntervalStatisticsAction action) => Action(async client =>
        {
            var parseConfigurationResult =
                from start in NonEmptyString.Create(fromDate)
                from end in NonEmptyString.Create(toDate)
                select DatePickerValidator.TryParseConfiguration(start, end, DateTimeZone.Utc);

            var configuration = DatePickerValidator.ParseConfigOrGetDefault(parseConfigurationResult);

            var errorMessage = DatePickerValidator.GetPossibleErrorMsg(parseConfigurationResult);
            
            var instantInterval = configuration.Interval.ToInstantInterval();
            var prs = await pullRequestRepository.GetAllIntersecting(client.CurrentRepositoryId, instantInterval);
            var pullRequests = prs.ToImmutableList();
            
            var charts = action(configuration, pullRequests);
            
            return View("Statistics", new PullRequestStatisticsViewModel(configuration, pullRequests, errorMessage, charts));
        });
       
        private static LineDataset CreateDataSet(DateInterval interval, DataCube1<LocalDate, double> data, LineDataSetConfiguration configuration)
        {
            var color = configuration.Color.ToArgbString();
            var colorList = new List<string> { color };
            var dataSetData = interval.Select(date => data.Get(date).GetOrElse(double.NaN)).ToList();
            return new LineDataset
            {
                Label = configuration.Label,
                Data = dataSetData,
                BorderColor = color,
                BackgroundColor = color,
                PointBorderColor = colorList,
                PointHoverBorderColor = colorList,
                PointBackgroundColor = colorList,
                PointHoverBackgroundColor = colorList,
                Fill = "false"
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Issue;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.Commits;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Commit;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Extensions;
using Chart = CodeInsight.Web.Common.Charts.Chart;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Web.Controllers
{
    public class CommitController : AuthorizedController
    {
        private readonly ICommitRepository commitRepository;
        private readonly IIssueRepository issueRepository;

        public CommitController(ICommitRepository commitRepository, IIssueRepository issueRepository, ClientAuthenticator clientAuthenticator) : base(clientAuthenticator)
        {
            this.commitRepository = commitRepository;
            this.issueRepository = issueRepository;
        }
        
        #region OverTimeTab

        public Task<IActionResult> OverTimeTab(string fromDate, string toDate) => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            
            //TODO: Refactor
            var parseConfigurationResult =
                from start in NonEmptyString.Create(fromDate)
                from end in NonEmptyString.Create(toDate)
                select ParseConfiguration(start, end, DateTimeZone.Utc);

            var configuration = parseConfigurationResult
                .FlatMap(c => c.Success)
                .GetOrElse(CreateDefaultConfiguration());

            var errorMessage = parseConfigurationResult
                .FlatMap(c => c.Error)
                .Map(ToErrorMessage);
            //end

            var interval = new DateInterval(
                configuration.Interval.Start,
                configuration.Interval.End);

            return View("OverTimeView", 
                new OverTimeModel(
                    configuration,
                    errorMessage,
                    ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval)))
                );
        });

        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            var maxInterval = new DateInterval(
                commits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                commits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);


            var commitsCube = new DataCube1<LocalDate, double>();
            var selectedWeekCube = new DataCube1<LocalDate, double>();

            var statsAll = GetDayStats(commits, maxInterval);
            foreach (var stat in statsAll)
            {
                commitsCube.Set(stat.Day, stat.CommitCount);
                if (interval.Contains(stat.Day))
                {
                    selectedWeekCube.Set(stat.Day, stat.CommitCount);
                }
            }

            yield return Chart.FromInterval(
                "Number of Commits on Selected Interval",
                interval,
                new List<Dataset>() { CreateDataSet(interval, selectedWeekCube, config) },
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );

            yield return Chart.FromInterval(
                "All Time Numbers of Commits",
                maxInterval,
                new List<Dataset>() { CreateDataSet(maxInterval, commitsCube, config) }, 
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );
        }
        
        #endregion

        //Abandoned
        #region ByTaskTab

        public Task<IActionResult> PerTaskTable() => Action(async client => View("FeatureUnavailable"));

        #endregion

        #region CodeTab

        public Task<IActionResult> CodeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();

            var maxInterval = new DateInterval(
                commits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                commits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);

            return View("CodeTabView",
                new CodeTabModel(ImmutableList.CreateRange(CreateCodeCharts(commits, maxInterval))));
        });

        private static IEnumerable<Chart> CreateCodeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var configAdded = new LineDataSetConfiguration("Additions", Color.Green);
            var configDeleted = new LineDataSetConfiguration("Deletions", Color.DarkRed);

            var stats = GetDayStats(commits, interval);
            var addedCube = new DataCube1<LocalDate, double>();
            var deletedCube = new DataCube1<LocalDate, double>();
            foreach (var stat in stats)
            {
                addedCube.Set(stat.Day, stat.Additions);
                deletedCube.Set(stat.Day, stat.Deletions);
            }

            yield return Chart.FromInterval(
                "All Time Code Changes",
                interval,
                new List<Dataset>()
                    { CreateDataSet(interval, addedCube, configAdded),
                      CreateDataSet(interval, deletedCube, configDeleted) },
                xAxis: NonEmptyString.Create("Dates").Get(),
                yAxis: NonEmptyString.Create("Number of Code Changes").Get()
            );
        }
        
        #endregion

        #region AuthorTab

        public Task<IActionResult> AuthorTable() => Action( async client =>
        {
            var commits = commitRepository.GetAll();
            var authors = commits.Result.Select(cm => cm.AuthorName).Distinct();

            var stats = authors.Select(author => AuthorCalculator.PerAuthor(commits.Result, author)).ToList();

            return View("AuthorView", new AuthorViewModel(ImmutableList.CreateRange(stats)));
        });

        #endregion

        #region Common
        
        private static IEnumerable<DayStats> GetDayStats(IEnumerable<Commit> commits, DateInterval interval)
        {
            var day = interval.Start;
            for (int i = 0; i < interval.Length; i++)
            {
                yield return DayCalculator.PerDay(commits, day);
                day = day.Plus(Period.FromDays(1));
            }
        }

        public enum ConfigurationError
        {
            InvalidFromDate,
            InvalidToDate,
            ToDateIsAfterFrom,
            ToDateIsAfterTomorrow
        }
        private static string ToErrorMessage(ConfigurationError error)
        {
            return error.Match(
                ConfigurationError.InvalidFromDate, _ => "Invalid Start date.",
                ConfigurationError.InvalidToDate, _ => "Invalid End date.",
                ConfigurationError.ToDateIsAfterFrom, _ => "Start cannot be after end.",
                ConfigurationError.ToDateIsAfterTomorrow, _ => "End cannot be after tomorrow."
            );
        }

        //TODO: Refactor Interval parser
        private static OTStatsConfig CreateDefaultConfiguration()
        {
            var end = SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset().Date.ToLocalDateTime().Date;
            var start = end.PlusDays(-7);
            return new OTStatsConfig(new DateInterval(start, end));
        }

        private static ITry<OTStatsConfig, ConfigurationError> ParseConfiguration(
            NonEmptyString fromDate,
            NonEmptyString toDate,
            DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var maxToDate = now.InZone(zone).Date.PlusDays(1);

            var start = ParseDate(fromDate).ToTry(_ => ConfigurationError.InvalidFromDate);
            var end = ParseDate(toDate)
                .ToTry(_ => ConfigurationError.InvalidToDate)
                .FlatMap(d => (d <= maxToDate).ToTry(t => d, f => ConfigurationError.ToDateIsAfterTomorrow));

            return
                from e in end
                from s in start
                from interval in CreateInterval(s, e).ToTry(_ => ConfigurationError.ToDateIsAfterFrom)
                select new OTStatsConfig(interval);
        }

        private static IOption<DateInterval> CreateInterval(LocalDate start, LocalDate end)
        {
            return end < start ? None<DateInterval>() : Some(new DateInterval(start, end));
        }

        private static IOption<LocalDate> ParseDate(string date)
        {
            var dateIsValid = DateTimeOffset.TryParseExact(date, "dd/MM/yyyy", null, DateTimeStyles.AssumeLocal, out var result);
            var resultAsOffset = dateIsValid ? Some(result) : None<DateTimeOffset>();
            return resultAsOffset.Map(ZonedDateTime.FromDateTimeOffset).Map(d => d.Date);
        }

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
        
        #endregion
    }
}

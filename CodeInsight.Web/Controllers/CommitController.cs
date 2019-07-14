using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Issue;
using CodeInsight.Domain.Repository;
using CodeInsight.Library;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.Commits;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models;
using CodeInsight.Web.Models.Commit;
using CodeInsight.Web.Models.PullRequest;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public Task<IActionResult> OverTimeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();

            //TODO: Add the use of datepicker
            var interval = new DateInterval(
                commits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                commits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);

            //TODO: Add error msg and config
            return View("OverTimeView", 
                new OverTimeModel(
                    new IntervalStatisticsConfiguration(new ZonedDateInterval(interval, DateTimeZone.Utc), Instant.FromDateTimeUtc(DateTime.UtcNow) ), //change
                    null, //change
                    ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval)))
                );
        });

        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            
            var commitsCube = new DataCube1<LocalDate, double>();
            var selectedWeekCube = new DataCube1<LocalDate, double>();

            var statsAll = GetDayStats(commits, commits.Min(cm => cm.CommittedAt), commits.Max(cm => cm.CommittedAt));
            foreach (var stat in statsAll)
            {
                commitsCube.Set(stat.Day.ToDateTimeOffset().Date.ToLocalDateTime().Date, stat.CommitCount);
                if (interval.Contains(stat.Day.ToDateTimeOffset().Date.ToLocalDateTime().Date))
                {
                    selectedWeekCube.Set(stat.Day.ToDateTimeOffset().Date.ToLocalDateTime().Date, stat.CommitCount);
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
                new DateInterval(
                commits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                commits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date),
                new List<Dataset>() { CreateDataSet(interval, commitsCube, config) }, 
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

            var interval = new DateInterval(
                commits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                commits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);

            return View("CodeTabView",
                new CodeTabModel(ImmutableList.CreateRange(CreateCodeCharts(commits, interval))));
        });

        private static IEnumerable<Chart> CreateCodeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var configAdded = new LineDataSetConfiguration("Additions", Color.Green);
            var configDeleted = new LineDataSetConfiguration("Deletions", Color.DarkRed);

            var stats = GetDayStats(commits, commits.Min(cm => cm.CommittedAt), commits.Max(cm => cm.CommittedAt));
            var addedCube = new DataCube1<LocalDate, double>();
            var deletedCube = new DataCube1<LocalDate, double>();
            foreach (var stat in stats)
            {
                addedCube.Set(stat.Day.ToDateTimeOffset().Date.ToLocalDateTime().Date, stat.Additions);
                deletedCube.Set(stat.Day.ToDateTimeOffset().Date.ToLocalDateTime().Date, stat.Deletions);
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

        //COMMON
        //TODO: Fix calculation issue with the loop
        private static IEnumerable<DayStats> GetDayStats(IEnumerable<Commit> commits, Instant start, Instant end)
        {
            DateInterval a = new DateInterval(start.ToDateTimeOffset().Date.ToLocalDateTime().Date, end.ToDateTimeOffset().Date.ToLocalDateTime().Date);
            for (int i = 0; i < a.Length; i++)
            {
                yield return DayCalculator.PerDay(commits, start);
                start = start.Plus(Duration.FromDays(1));
            }
        }

        //TODO: interval parser
        private static IntervalStatisticsConfiguration CreateDefaultConfiguration(DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var end = now.InZone(zone).Date.PlusDays(1);
            var start = end.PlusDays(-30);
            return new IntervalStatisticsConfiguration(new ZonedDateInterval(new DateInterval(start, end), zone), now);
        }

        private static ITry<IntervalStatisticsConfiguration, PullRequestController.ConfigurationError> ParseConfiguration(
            NonEmptyString fromDate,
            NonEmptyString toDate,
            DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var maxToDate = now.InZone(zone).Date.PlusDays(1);

            var start = ParseDate(fromDate).ToTry(_ => PullRequestController.ConfigurationError.InvalidFromDate);
            var end = ParseDate(toDate)
                .ToTry(_ => PullRequestController.ConfigurationError.InvalidToDate)
                .FlatMap(d => (d <= maxToDate).ToTry(t => d, f => PullRequestController.ConfigurationError.ToDateIsAfterTomorrow));

            return
                from e in end
                from s in start
                from interval in CreateInterval(s, e).ToTry(_ => PullRequestController.ConfigurationError.ToDateIsAfterFrom)
                select new IntervalStatisticsConfiguration(new ZonedDateInterval(interval, zone), now);
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
    }
}

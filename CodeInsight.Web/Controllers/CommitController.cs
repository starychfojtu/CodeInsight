using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.Linq;
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

        //TODO: OverTimeTab
        //TODO: REFACTOR - add GetWeeks > send it to <option>
        public Task<IActionResult> OverTimeTab() => Action(async client =>
        {
            //var commits = commitRepository.GetAll();

            //TEMP
            var commits = new List<Commit>
            {
                new Commit(NonEmptyString.Create("1").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow), NonEmptyString.Create("CommitMsg").Get()),
                new Commit(NonEmptyString.Create("2").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 2").Get(), NonEmptyString.Create("2").Get(), 1, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(3)),
                    NonEmptyString.Create("CommitMsg 2").Get()),
                new Commit(NonEmptyString.Create("3").Get(), NonEmptyString.Create("12").Get(),
                    NonEmptyString.Create("Tester 1").Get(), NonEmptyString.Create("1").Get(), 1, 2,
                    Instant.FromDateTimeOffset(DateTime.UtcNow).Minus(Duration.FromDays(2)),
                    NonEmptyString.Create("CommitMsg 3").Get())
            };

            //TODO: View based choosing of week
            var interval = new DateInterval(LocalDate.MinIsoValue, LocalDate.MaxIsoValue);

            return View("OverTimeStatisticsView", 
                new WeekViewModel(ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval))));
        });

        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            var statsAll = GetAllWeekStats(commits);
            var statsWeek = GetDayStats(commits, option.Day, option.Count);

            //TODO: Correctly use graph/chart
            yield return Chart.FromInterval(
                "All Time",
                interval,
                new List<Dataset>()
                { CreateCommitDataSet(interval, statsAll, config) }, 
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );

            var weeks = "test";
            yield return Chart.FromInterval(
                "Week" + weeks,
                interval,
                new List<Dataset>()
                { CreateCodeDataSet(interval, statsWeek, config) }, 
                xAxis: NonEmptyString.Create("Days").Get(),
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
            var interval = new DateInterval(LocalDate.MinIsoValue, LocalDate.MaxIsoValue);

            return View("OverTimeStatisticsView",
                new WeekViewModel(ImmutableList.CreateRange(CreateCodeCharts(commits, interval))));
        });

        private static IEnumerable<Chart> CreateCodeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var configAdded = new LineDataSetConfiguration("Number of commits", Color.Green);
            var configDeleted = new LineDataSetConfiguration("Number of commits", Color.DarkRed);
            var stats = GetAllWeekStats(commits);

            yield return Chart.FromInterval(
                "All Time Code",
                interval,
                new List<Dataset>()
                    { CreateCodeDataSet(interval, stats, configAdded),
                      CreateCodeDataSet(interval, stats, configDeleted) },
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of Code Changes").Get()
            );
        }
        
        #endregion

        #region AuthorTab

        public Task<IActionResult> AuthorTable() => Action( async client =>
        {
            var commits = commitRepository.GetAll();
            var authors = commits.Result.Select(cm => cm.AuthorName);

            var stats = authors.Select(author => AuthorCalculator.PerAuthor(commits.Result, author)).ToList();

            return View("AuthorView", new AuthorViewModel(ImmutableList.CreateRange(stats)));
        });

        #endregion

        //COMMON
        private static IEnumerable<WeekStats> GetAllWeekStats(IEnumerable<Commit> commits)
        {
            var current = commits.Min(cm => cm.CommittedAt);
            var max = commits.Max(cm => cm.CommittedAt);


            while (max.ToDateTimeUtc().ToLocalDateTime() > current.ToDateTimeUtc().ToLocalDateTime())
            {
                var interval = new DateInterval(current.ToDateTimeOffset(), );
                var period = Period.Between(current.ToDateTimeUtc().ToLocalDateTime(), LocalDateTime.Min(max.ToDateTimeUtc().ToLocalDateTime(), current.ToDateTimeUtc().ToLocalDateTime().Next(IsoDayOfWeek.Sunday))).Days;
                yield return WeekCalculator.Calculate(GetDayStats(commits, current, period).ToImmutableList());
                //TODO: Correct time change
                //current = current.ToDateTimeUtc().ToLocalDateTime().Next(IsoDayOfWeek.Sunday);
            }
        }

        private static IEnumerable<DayStats> GetDayStats(IEnumerable<Commit> commits, Instant day, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return DayCalculator.PerDay(commits, day);
                day.Plus(Duration.FromDays(1));
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


        private static LineDataset CreateCommitDataSet(DateInterval interval, IEnumerable<WeekStats> data, LineDataSetConfiguration configuration)
        {
            var color = configuration.Color.ToArgbString();
            var colorList = new List<string> { color };
            var dataSetData = interval.Select(date => data.Where(stat => stat.FirstDay == date).Select(stat => stat.CommitCount).GetOrElse(double.NaN)).ToList();
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

        private static LineDataset CreateCodeDataSet(DateInterval interval, IEnumerable<WeekStats> data, LineDataSetConfiguration configuration)
        {
            var color = configuration.Color.ToArgbString();
            var colorList = new List<string> { color };
            //TODO: Generalize or copy-paste
            var dataSetData = interval.Select(date => data.Where(stat => stat.FirstDay == date).Select(stat => stat.Additions).GetOrElse(double.NaN)).ToList();

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

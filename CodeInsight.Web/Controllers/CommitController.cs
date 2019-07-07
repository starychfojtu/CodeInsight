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
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models;
using CodeInsight.Web.Models.Commit;
using CodeInsight.Web.Models.PullRequest;
using FuncSharp;
using Microsoft.AspNetCore.Http;
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
        public Task<IActionResult> OverTimeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            var interval = new DateInterval(LocalDate.MinIsoValue, LocalDate.MaxIsoValue);

            return View("OverTimeStatisticsView", 
                new WeekViewModel(ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval))));
        });

        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            var stats = GetAllWeekStats(commits);

            //TODO: Correctly use graph/chart
            yield return Chart.FromInterval(
                "All Time",
                interval,
                stats.Select(week => CreateDataSet(interval, new DataCube1<LocalDate, double>(), config)).ToList(),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );

            var weeks = "test";
            yield return Chart.FromInterval(
                "Week" + weeks,
                interval,
                stats.Select(kvp => CreateDataSet(interval, new DataCube1<LocalDate, double>(), config)).ToList(),
                xAxis: NonEmptyString.Create("Days").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );
        }

        #endregion

        //Abandoned
        #region ByTaskTab

        public Task<IActionResult> PerTaskTable() => Action(async client =>
        {
            return View("FeatureUnavailable");
        });

        #endregion

        #region CodeTab

        public Task<IActionResult> CodeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            var interval = new DateInterval(LocalDate.MinIsoValue, LocalDate.MaxIsoValue);

            return View("OverTimeStatisticsView",
                new WeekViewModel(ImmutableList.CreateRange(CreateCodeCharts(commits, interval))));
        });

        //TODO: CodeTab
        private static IEnumerable<Chart> CreateCodeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var config = new LineDataSetConfiguration("Number of commits", Color.DarkRed);
            var stats = GetAllWeekStats(commits);

            yield return Chart.FromInterval(
                "All Time Code",
                interval,
                stats.Select(week => CreateDataSet(interval, new DataCube1<LocalDate, double>(), config)).ToList(),
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
                var period = Period.Between(current.ToDateTimeUtc().ToLocalDateTime(), LocalDateTime.Min(max.ToDateTimeUtc().ToLocalDateTime(), current.ToDateTimeUtc().ToLocalDateTime().Next(IsoDayOfWeek.Sunday))).Days;
                yield return WeekCalculator.Calculate(GetDayStats(commits, current, period));
                current = current.ToDateTimeUtc().ToLocalDateTime().Next(IsoDayOfWeek.Sunday).ToDateTimeUnspecified().ToInstant();
            }
        }

        private static IEnumerable<DayStats> GetDayStats(IEnumerable<Commit> commits, Instant day, int count)
        {
            for (int i = 0; i < count - 1; i++)
            {
                yield return DayCalculator.PerDay(commits, day);
                day.Plus(Duration.FromDays(1));
            }
        }

        //TODO: Refactor
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

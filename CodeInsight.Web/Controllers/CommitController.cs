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
            var interval = new DateInterval(LocalDate.MinIsoValue, LocalDate.MaxIsoValue );

            return View("OverTimeStatisticsView", 
                new WeekViewModel(ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval))));
        });
        
        //TODO: CodeTab
        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            //TODO: Call get
            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            var dates = commits.Select(cm => cm.CommittedAt);
            var stats = dates.Select(entry => DayCalculator.PerDay(commits, entry)).ToList();
            stats.Add(new DayStats(Instant.FromDateTimeOffset(DateTimeOffset.Now), 10,5, 2));
            stats.Add(new DayStats(Instant.FromDateTimeOffset(DateTimeOffset.MinValue), 7, 5, 2));
            var chartDataAllTime = stats
                .Select(c => new LineScatterData
                {
                    y = c.Additions.ToString(),
                    x = c.Day.ToString()
                });

            var chartDataWeek = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            //TODO: Correctly use graph/chart
            yield return Chart.FromInterval(
                "All Time",
                interval,
                chartDataAllTime.Select(kvp => CreateDataSet(interval, new DataCube1<LocalDate, double>(), config)).ToList(),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );

            //TODO: Use correct graph/chart
            var week = "test";
            yield return new Chart(
                "Week " + week,
                ChartType.Scatter,
                Chart.CreateScatterData("Number of commits", chartDataWeek),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );
        }

        #endregion

        //Abandoned
        #region ByTaskTab

        /*/
        public Task<IActionResult> PerTaskTable() => Action(async client =>
        {
            return View("TestView");
        });
        /**/

        #endregion


        #region CodeTab

        public Task<IActionResult> CodeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            
            return View("OverTimeStatisticsView",
                new WeekViewModel(ImmutableList.CreateRange(CreateCodeCharts(commits))));
        });

        //TODO: CodeTab
        private static IEnumerable<Chart> CreateCodeCharts(IEnumerable<Commit> commits)
        {
            var chartDataAllTimeCode = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            yield return new Chart(
                "All Time Code",
                ChartType.Scatter,
                Chart.CreateScatterData("Number of Code Changes", chartDataAllTimeCode),
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
        //TODO: Refactor
        private static IEnumerable<WeekStats> GetWeekStats(IEnumerable<Commit> commits)
        {
            //TODO: per week get week stats, use per day GetDayStats
            yield break;
        }

        private static IEnumerable<WeekStats> GetDayStats(IEnumerable<Commit> commits)
        {
            //TODO: per day, per week get week stats
            yield break;
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

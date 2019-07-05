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

        //TODO: CodeTab
        #region OverTimeTab
        public Task<IActionResult> OverTimeTab() => Action(async client =>
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var finiteInterval = new FiniteInterval(
                now.Minus(Duration.FromDays(365)),
                now
            );

            var commits = await commitRepository.GetAll();
            var dataAllTime = new List<WeekStats>();
            foreach (var entry in commits)
            {
                
            }
            var chartDataAllTime = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            var chartDataWeek = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            var listOfCharts = new List<Chart>();

            var chartAllTime = new Chart(
                "All Time",
                ChartType.Scatter,
                Chart.CreateScatterData("Number of commits", chartDataAllTime),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
                );
            listOfCharts.Add(chartAllTime);


            var week = "test";
            var chartWeek = new Chart(
                "Week " + week,
                ChartType.Scatter,
                Chart.CreateScatterData("Number of commits", chartDataWeek),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );
            listOfCharts.Add(chartWeek);

            return View("OverTimeStatisticsView", new WeekViewModel(ImmutableList.CreateRange(listOfCharts)));
        });
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

        //TODO: CodeTab
        #region CodeTab

        public Task<IActionResult> CodeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            var dataAllTime = new List<WeekStats>();
            foreach (var entry in commits)
            {

            }

            var chartDataAllTimeCode = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            var chartAllTimeCode = new Chart(
                "All Time Code",
                ChartType.Scatter,
                Chart.CreateScatterData("Number of commits", chartDataAllTimeCode),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
            );

            return View("OverTimeStatisticsView", new WeekViewModel(ImmutableList.Create(chartAllTimeCode)));
        });

        #endregion

        #region AuthorTab
        public Task<IActionResult> AuthorTable() => Action(async client =>
        {
            var commits = commitRepository.GetAll();
            var authors = commits.Result.Select(cm => cm.AuthorName);

            var stats = new List<AuthorStats>();
            foreach (var author in authors)
            {
                stats.Add(AuthorCalculator.PerAuthor(commits.Result, author));
            }

            return View("AuthorView", new AuthorViewModel(ImmutableList.CreateRange(stats)));
        });
        #endregion
    }
}

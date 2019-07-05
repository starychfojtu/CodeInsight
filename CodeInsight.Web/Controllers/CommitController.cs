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
using CodeInsight.PullRequests;
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
    //TODO: Controller for commits
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
        //Testing
        /**/
        public Task<IActionResult> OverTimeTab() => Action(async client =>
        {
            /*/
            var now = SystemClock.Instance.GetCurrentInstant();
            var finiteInterval = new FiniteInterval(
                now.Minus(Duration.FromDays(365)),
                now
            );

            var commits = await commitRepository.GetAllByAuthor(NonEmptyString.Create("name").Get());
            var data = commits
                .Select(c => new LineScatterData
                {
                    y = 128.ToString(),
                    x = 0.ToString()
                });

            var chart = new Chart(
                "Title",
                ChartType.Scatter,
                Chart.CreateScatterData("Number of commits", data),
                xAxis: NonEmptyString.Create("Week").Get(),
                yAxis: NonEmptyString.Create("Number of commits").Get()
                );
                
            return View(new WeekViewModel(ImmutableList.Create(chart)));
            /**/
            return View("TestView");
        });
        /**/
        //TODO: Add enum charts

        #endregion

        /*/
        #region ByTaskTab
        public Task<IActionResult> PerTaskTable() => Action(async client =>
        {
            return View("TestView");
        });

        #endregion
        /**/

        #region AuthorTab

        public Task<IActionResult> AuthorTable() => Action(async client =>
        {
            //return View("AuthorView");
            return View("TestView");
        });

        #endregion
    }
}

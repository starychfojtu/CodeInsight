using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using CodeInsight.Domain.Commit;
using CodeInsight.Domain.Issue;
using CodeInsight.Library.Types;
using CodeInsight.Commits;
using CodeInsight.Library.DatePicker;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Charts;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Commit;
using FuncSharp;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NodaTime.Extensions;
using Chart = CodeInsight.Web.Common.Charts.Chart;

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

            var parseConfigurationResult =
                from start in NonEmptyString.Create(fromDate)
                from end in NonEmptyString.Create(toDate)
                select DatePickerValidator.TryParseConfiguration(start, end, DateTimeZone.Utc);

            var errorMessage = DatePickerValidator.GetPossibleErrorMsg(parseConfigurationResult);
            var configuration = DatePickerValidator.ParseConfigOrGetDefault(parseConfigurationResult);

            var interval = new DateInterval(
                configuration.Interval.Start.Date,
                configuration.Interval.End.Date);

            return View("OverTimeView",
                new OverTimeModel(
                    configuration,
                    errorMessage,
                    ImmutableList.CreateRange(CreateOverTimeCharts(commits, interval)))
                );
        });

        private static IEnumerable<Chart> CreateOverTimeCharts(IEnumerable<Commit> commits, DateInterval interval)
        {
            var enumeratedCommits = commits.ToList();

            var config = new LineDataSetConfiguration("Number of commits", Color.Cyan);
            var maxInterval = new DateInterval(
                enumeratedCommits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                enumeratedCommits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);


            var commitsCube = new DataCube1<LocalDate, double>();
            var selectedWeekCube = new DataCube1<LocalDate, double>();

            var statsAll = GetDayStats(enumeratedCommits, maxInterval);
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

        #region ByTaskTab

        public Task<IActionResult> PerTaskTable() => Action(async client =>
        {
            var issues = await issueRepository.GetAll();

            return View("IssueView", new IssueViewModel(ImmutableList.CreateRange(issues)));
        });

        #endregion

        #region CodeTab

        public Task<IActionResult> CodeTab() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            var enumeratedCommits = commits.ToList();

            var maxInterval = new DateInterval(
                enumeratedCommits.Min(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date,
                enumeratedCommits.Max(cm => cm.CommittedAt).ToDateTimeOffset().Date.ToLocalDateTime().Date);

            return View("CodeTabView",
                new CodeTabModel(ImmutableList.CreateRange(CreateCodeCharts(enumeratedCommits, maxInterval))));
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
                deletedCube.Set(stat.Day, -stat.Deletions);
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

        public Task<IActionResult> AuthorTable() => Action(async client =>
        {
            var commits = await commitRepository.GetAll();
            var enumeratedCommits = commits.ToList();

            var authors = enumeratedCommits.Select(cm => cm.AuthorName).Distinct();

            var stats = authors.Select(author => AuthorCalculator.PerAuthor(enumeratedCommits, author)).ToList();

            return View("AuthorView", new AuthorViewModel(ImmutableList.CreateRange(stats)));
        });

        #endregion

        #region Common

        private static IEnumerable<DayStats> GetDayStats(IEnumerable<Commit> commits, DateInterval interval)
        {
            var enumeratedCommits = commits.ToList();

            var day = interval.Start;

            for (int i = 0; i < interval.Length; i++)
            {
                yield return DayCalculator.PerDay(enumeratedCommits, day);
                day = day.Plus(Period.FromDays(1));
            }
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

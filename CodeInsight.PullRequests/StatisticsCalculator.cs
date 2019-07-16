using System.Collections.Generic;
using System.Collections.Immutable;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library.DatePicker;
using CodeInsight.Library.Extensions;
using FuncSharp;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public static class StatisticsCalculator
    {
        public static IntervalStatistics Calculate(IEnumerable<PullRequest> pullRequests, IntervalStatisticsConfiguration configuration)
        {
            var endDate = configuration.Interval.End;
            var statistics = new DataCube1<LocalDate, IImmutableSet<PullRequest>>();
            foreach (var pullRequest in pullRequests)
            {
                var dates = GetInfluencingDates(pullRequest, endDate);
                foreach (var date in dates)
                {
                    statistics.SetOrElseUpdate(date, ImmutableHashSet.Create(pullRequest), (a, b) => a.Union(b));
                }
            }

            return new IntervalStatistics(statistics, configuration);
        }

        private static DateInterval GetInfluencingDates(PullRequest pullRequest, ZonedDateTime maxEnd) =>
            new DateInterval(
                pullRequest.CreatedAt.InZone(maxEnd.Zone).Date,
                pullRequest.End
                    .Map(e => e.InZone(maxEnd.Zone).Date)
                    .GetOrElse(maxEnd.Date)
                    .Min(maxEnd.Date)
            );
    }
}
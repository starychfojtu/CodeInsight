using System.Collections.Generic;
using System.Collections.Immutable;
using CodeInsight.Domain.Commit;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Commits
{
    public static class OverTimeCalculator
    {
        public static OverTimeStats Calculate(IEnumerable<Commit> commits, ZonedDateTime startDate)
        {
            var stats = new DataCube1<LocalDate, IImmutableSet<Commit>>();
            //TODO: Add a way of setting the duration to 7
            Duration duration = new Duration();
            foreach (var commit in commits)
            {
                var dates = GetDates(commit, startDate.Plus(duration));

                foreach (var date in dates)
                {
                    stats.SetOrElseUpdate(date, ImmutableHashSet.Create(commit), (a, b) => a.Union(b));
                }

            }
            return new OverTimeStats(stats);
        }

        private static DateInterval GetDates(Commit commit, ZonedDateTime maxEnd)
        {
            return new DateInterval(
                commit.CommittedAt.InZone(maxEnd.Zone).Date, 
                LocalDate.Min(commit.CommittedAt.InZone(maxEnd.Zone).Date, maxEnd.Date));
        }
    }
}

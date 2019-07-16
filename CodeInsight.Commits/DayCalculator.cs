using System.Collections.Generic;
using System.Linq;
using CodeInsight.Domain.Commit;
using NodaTime;

namespace CodeInsight.Commits
{
    public static class DayCalculator
    {
        public static DayStats PerDay(IEnumerable<Commit> commits, LocalDate day)
        {
            var commitsSpecified = commits
                .Where(cm => cm.CommittedAt.ToDateTimeOffset().Date == day.ToDateTimeUnspecified())
                .ToList();
            var count = commitsSpecified.Count;
            var additions = commitsSpecified
                .Sum(cm => cm.Additions);
            var deletions = commitsSpecified
                .Sum(cm => cm.Deletions);

            return new DayStats(
                day: day, 
                additions: (uint) additions, 
                deletions: (uint) deletions, 
                commitCount: (uint) count
                );
        }
    }
}

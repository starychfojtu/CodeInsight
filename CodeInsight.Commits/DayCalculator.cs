using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeInsight.Domain.Commit;
using NodaTime;

namespace CodeInsight.Commits
{
    public static class DayCalculator
    {
        public static DayStats PerDay(IEnumerable<Commit> commits, Instant day)
        {
            var count = (uint) commits.Count(cm => cm.CommittedAt.ToDateTimeOffset().Date == day.ToDateTimeOffset().Date);
            var additions = commits
                .Where(cm => cm.CommittedAt.ToDateTimeOffset().Date == day.ToDateTimeOffset().Date)
                .Sum(cm => cm.Additions);
            var deletions = commits
                .Where(cm => cm.CommittedAt.ToDateTimeOffset().Date == day.ToDateTimeOffset().Date)
                .Sum(cm => cm.Deletions);

            return new DayStats(
                day: day, 
                additions: (uint) additions, 
                deletions: (uint) deletions, 
                commitCount: count
                );
        }
    }
}

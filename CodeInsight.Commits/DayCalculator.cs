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
            var count = (uint) commits.Count(cm => cm.CommittedAt == day);
            var additions = commits
                .Where(cm => cm.CommittedAt == day)
                .Select(cm => cm.Additions)
                .Aggregate<uint, uint>(0, (current, entry) => current + entry);
            var deletions = commits
                .Where(cm => cm.CommittedAt == day)
                .Select(cm => cm.Deletions)
                .Aggregate<uint, uint>(0, (current, entry) => current + entry);

            return new DayStats(day, additions, deletions, count);
        }
    }
}

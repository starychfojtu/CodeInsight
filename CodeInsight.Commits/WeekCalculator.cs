using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeInsight.Domain.Commit;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Commits
{
    public static class WeekCalculator
    {
        public static WeekStats Calculate(IEnumerable<Commit> commits, Interval interval)
        {
            var count = (uint) commits.Count(cm => interval.Contains(cm.CommittedAt));
            var additions = commits
                .Where(cm => interval.Contains(cm.CommittedAt))
                .Select(cm => cm.Additions)
                .Aggregate((uint) 0, (current, entry) => current + entry);
            var deletion = commits
                .Where(cm => interval.Contains(cm.CommittedAt))
                .Select(cm => cm.Deletions)
                .Aggregate((uint)0, (current, entry) => current + entry);

            return new WeekStats(additions, deletion, count);
        }
    }
}

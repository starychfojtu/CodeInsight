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
        public static WeekStats Calculate(IEnumerable<DayStats> stats)
        {
            var count = (uint) stats.Count();
            var additions = stats
                .Select(cm => cm.Additions)
                .Aggregate((uint) 0, (current, entry) => current + entry);
            var deletion = stats
                .Select(cm => cm.Deletions)
                .Aggregate((uint)0, (current, entry) => current + entry);

            return new WeekStats(additions, deletion, count);
        }
    }
}

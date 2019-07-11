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
        public static WeekStats Calculate(IImmutableList<DayStats> stats)
        {
            var count = stats.Count();
            var additions = stats.Sum(cm => cm.Additions);
            var deletion = stats.Sum(cm => cm.Deletions);

            return new WeekStats(stats.Min(st => st.Day), (uint)stats.Count(), (uint) additions, (uint) deletion, (uint) count);
        }
    }
}

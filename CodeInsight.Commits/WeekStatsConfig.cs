using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Commits
{
    //TODO: Correct config
    public sealed class WeekStatsConfig
    {
        public WeekStatsConfig(Instant intervalStart, Instant intervalEnd)
        {
            IntervalStart = intervalStart;
            IntervalEnd = intervalEnd;
        }

        public ZonedDateInterval Interval { get; }

        public Instant IntervalStart { get; }

        public Instant IntervalEnd { get; }
    }
}

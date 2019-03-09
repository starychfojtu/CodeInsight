using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public sealed class IntervalStatisticsConfiguration
    {
        public IntervalStatisticsConfiguration(ZonedDateInterval interval, Instant calculateAt)
        {
            Interval = interval;
            CalculateAt = calculateAt;
        }

        public ZonedDateInterval Interval { get; }
        
        public Instant CalculateAt { get; }
    }
}
using NodaTime;

namespace CodeInsight.Commits
{
    public sealed class OTStatsConfig
    {
        public OTStatsConfig(DateInterval interval)
        {
            Interval = interval;
        }

        public DateInterval Interval { get; }
    }
}

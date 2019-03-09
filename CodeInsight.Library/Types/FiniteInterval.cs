using NodaTime;

namespace CodeInsight.Library.Types
{
    public sealed class FiniteInterval : NewType<Interval>
    {
        public FiniteInterval(Instant start, Instant end) : base(new Interval(start, end))
        {
        }

        public Instant Start => Value.Start;
        public Instant End => Value.End;
        public Duration Duration => Value.Duration;
    }
}
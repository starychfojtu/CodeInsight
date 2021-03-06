using FuncSharp;
using NodaTime;

namespace CodeInsight.Library.Extensions
{
    public static class NodaTimeExtensions
    {
        public static bool Intersects(this Interval a, Interval b)
        {
            var aStart = a.SafeStart().Map(s => s.ToUnixTimeTicks()).GetOrElse(long.MinValue);
            var bStart = b.SafeStart().Map(s => s.ToUnixTimeTicks()).GetOrElse(long.MinValue);
            var aEnd = a.SafeEnd().Map(s => s.ToUnixTimeTicks()).GetOrElse(long.MaxValue);
            var bEnd = b.SafeEnd().Map(s => s.ToUnixTimeTicks()).GetOrElse(long.MaxValue);
            return bEnd >= aStart && bStart <= aEnd;
        }

        public static IOption<Instant> SafeEnd(this Interval interval) =>
            interval.HasEnd ? Prelude.Some(interval.End) : Prelude.None<Instant>();
        
        public static IOption<Instant> SafeStart(this Interval interval) =>
            interval.HasEnd ? Prelude.Some(interval.End) : Prelude.None<Instant>();
    }
}
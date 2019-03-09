using FuncSharp;
using NodaTime;

namespace CodeInsight.Library.Types
{
    public class ZonedDateInterval : Product2<DateInterval, DateTimeZone>
    {
        public ZonedDateInterval(DateInterval dateInterval, DateTimeZone zone) : base(dateInterval, zone) {}

        public DateInterval DateInterval => ProductValue1;
        public DateTimeZone Zone => ProductValue2;

        public ZonedDateTime Start =>
            DateInterval.Start.AtStartOfDayInZone(Zone);

        public ZonedDateTime End =>
            DateInterval.End.AtStartOfDayInZone(Zone);
    }
}
using NodaTime;

namespace CodeInsight.Library
{
    public class ZonedDateInterval
    {
        public ZonedDateInterval(DateInterval dateInterval, DateTimeZone zone)
        {
            DateInterval = dateInterval;
            Zone = zone;
        }

        public DateInterval DateInterval { get; }
        
        public DateTimeZone Zone { get; }

        public ZonedDateTime Start =>
            DateInterval.Start.AtStartOfDayInZone(Zone);

        public ZonedDateTime End =>
            DateInterval.End.AtStartOfDayInZone(Zone);
    }
}
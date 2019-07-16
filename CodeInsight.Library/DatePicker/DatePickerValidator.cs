using System;
using System.Globalization;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Library.DatePicker
{
    public sealed class DatePickerValidator
    {
        public static IOption<string> GetPossibleErrorMsg(IOption<ITry<IntervalStatisticsConfiguration, ConfigurationError>> result)
        {
            return result
                .FlatMap(c => c.Error)
                .Map(ToErrorMessage);
        }

        public enum ConfigurationError
        {
            InvalidFromDate,
            InvalidToDate,
            ToDateIsAfterFrom,
            ToDateIsAfterTomorrow
        }

        private static string ToErrorMessage(ConfigurationError error)
        {
            return error.Match(
                ConfigurationError.InvalidFromDate, _ => "Invalid Start date.",
                ConfigurationError.InvalidToDate, _ => "Invalid End date.",
                ConfigurationError.ToDateIsAfterFrom, _ => "Start cannot be after end.",
                ConfigurationError.ToDateIsAfterTomorrow, _ => "End cannot be after tomorrow."
            );
        }

        public static ITry<IntervalStatisticsConfiguration, ConfigurationError> TryParseConfiguration(
            NonEmptyString fromDate,
            NonEmptyString toDate,
            DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var maxToDate = now.InZone(zone).Date.PlusDays(1);

            var start = ParseDate(fromDate).ToTry(_ => ConfigurationError.InvalidFromDate);
            var end = ParseDate(toDate)
                .ToTry(_ => ConfigurationError.InvalidToDate)
                .FlatMap(d => (d <= maxToDate).ToTry(t => d, f => ConfigurationError.ToDateIsAfterTomorrow));

            return
                from e in end
                from s in start
                from interval in CreateInterval(s, e).ToTry(_ => ConfigurationError.ToDateIsAfterFrom)
                select new IntervalStatisticsConfiguration(new ZonedDateInterval(interval, zone), now);
        }

        public static IntervalStatisticsConfiguration ParseConfigOrGetDefault(IOption<ITry<IntervalStatisticsConfiguration, ConfigurationError>> result)
        {

            return result
                .FlatMap(c => c.Success)
                .GetOrElse(CreateDefaultConfiguration(DateTimeZone.Utc));
        }

        private static IntervalStatisticsConfiguration CreateDefaultConfiguration(DateTimeZone zone)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var end = now.InZone(zone).Date.PlusDays(1);
            var start = end.PlusDays(-30);
            return new IntervalStatisticsConfiguration(new ZonedDateInterval(new DateInterval(start, end), zone), now);
        }



        private static IOption<DateInterval> CreateInterval(LocalDate start, LocalDate end)
        {
            return end < start ? Prelude.None<DateInterval>() : Prelude.Some(new DateInterval(start, end));
        }

        private static IOption<LocalDate> ParseDate(string date)
        {
            var dateIsValid = DateTimeOffset.TryParseExact(date, "dd/MM/yyyy", null, DateTimeStyles.AssumeLocal, out var result);
            var resultAsOffset = dateIsValid ? Prelude.Some(result) : Prelude.None<DateTimeOffset>();
            return resultAsOffset.Map(ZonedDateTime.FromDateTimeOffset).Map(d => d.Date);
        }
    }
}

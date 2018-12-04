using NodaTime;

namespace CodeInsight.Library
{
    public class LocalMonth
    {
        private LocalMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public int Year { get; }
        
        public int Month { get; }
        
        public static LocalMonth Create(LocalDate date) =>
            new LocalMonth(date.Year, date.Month);

        protected bool Equals(LocalMonth other) =>
            Year == other.Year && Month == other.Month;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LocalMonth m && Equals(m);
        }

        public override int GetHashCode() =>
            (Year * 397) ^ Month;
    }
}
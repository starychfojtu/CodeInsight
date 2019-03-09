using FuncSharp;
using NodaTime;

namespace CodeInsight.Library.Types
{
    public sealed class LocalMonth : Product2<int, int>
    {
        private LocalMonth(int year, int month) : base(year, month) {}

        public int Year => ProductValue1;
        public int Month => ProductValue2;
        
        public static LocalMonth Create(LocalDate date) =>
            new LocalMonth(date.Year, date.Month);
    }
}
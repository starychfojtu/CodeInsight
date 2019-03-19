using System;
using CodeInsight.Library;
using NodaTime;

namespace CodeInsight.Domain
{
    /// <summary>
    /// Represents amount of changes per time unit (hour), but in linear dependency.
    /// So that PR done by automatic refactoring with thousands of changes doesn't give you high efficiency.
    /// So instead of linear changes/time
    /// </summary>
    public sealed class Efficiency : NewType<double>
    {
        public Efficiency(double value) : base(value)
        {
        }

        public static Efficiency Create(uint changes, Duration time)
        {
            return new Efficiency(ChangesToSize(changes) / Math.Max(1, Math.Log10(time.TotalHours)));
        }

        /// <summary>
        /// Returns value between 0-100 indicating how big the changes are.
        /// </summary>
        private static double ChangesToSize(uint changes)
        {
            var expectedMaxPrSize = 1000f;
            var normalizedChanges = changes / expectedMaxPrSize;
            return Sigmoid(normalizedChanges) * 100;
        }
        
        private static double Sigmoid(double value) {
            return 1.0d / (1.0d + Math.Exp(-value));
        }
    }
}
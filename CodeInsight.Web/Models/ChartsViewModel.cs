using System.Collections.Immutable;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models
{
    public class ChartsViewModel
    {
        public ChartsViewModel(IImmutableList<Chart> charts)
        {
            Charts = charts;
        }

        public IImmutableList<Chart> Charts { get; }
    }
}
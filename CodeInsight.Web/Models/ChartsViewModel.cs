using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models
{
    public class ChartsViewModel
    {
        public ChartsViewModel(IReadOnlyList<Chart> charts)
        {
            Charts = charts;
        }

        public IReadOnlyList<Chart> Charts { get; }
    }
}
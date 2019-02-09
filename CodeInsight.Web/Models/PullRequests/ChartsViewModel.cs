using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.PullRequests
{
    public class ChartsViewModel
    {
        public ChartsViewModel(IEnumerable<Chart> charts)
        {
            Charts = charts;
        }

        public IEnumerable<Chart> Charts { get; }
    }
}
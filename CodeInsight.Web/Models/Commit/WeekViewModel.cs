using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.Commit
{
    public class WeekViewModel : ChartsViewModel
    {
        public WeekViewModel(IReadOnlyList<Chart> charts) : base(charts)
        {
        }
    }
}
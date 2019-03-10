using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.PullRequest
{
    public class EfficiencyViewModel : ChartsViewModel
    {
        public EfficiencyViewModel(IReadOnlyList<Chart> charts) : base(charts)
        {
        }
    }
}
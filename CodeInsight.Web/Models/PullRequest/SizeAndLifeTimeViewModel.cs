using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.PullRequest
{
    public class SizeAndLifeTimeViewModel : ChartsViewModel
    {
        public SizeAndLifeTimeViewModel(IReadOnlyList<Chart> charts) : base(charts)
        {
        }
    }
}
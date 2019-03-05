using System;
using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestIndexViewModel : ChartsViewModel
    {        
        public PullRequestIndexViewModel(DateTimeOffset from, IReadOnlyList<Chart> charts) : base(charts)
        {
            From = from;
        }
        
        public DateTimeOffset From { get; }
    }
}
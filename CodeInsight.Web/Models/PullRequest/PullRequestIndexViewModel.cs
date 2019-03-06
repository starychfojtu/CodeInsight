using System;
using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestIndexViewModel : ChartsViewModel
    {        
        public PullRequestIndexViewModel(DateTimeOffset from, IReadOnlyList<PullRequests.PullRequest> pullRequests, IReadOnlyList<Chart> charts) : base(charts)
        {
            From = from;
            PullRequests = pullRequests;
        }
        
        public DateTimeOffset From { get; }
        
        public IReadOnlyList<PullRequests.PullRequest> PullRequests { get; }
    }
}
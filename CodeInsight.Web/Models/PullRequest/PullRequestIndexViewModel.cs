using System.Collections.Generic;
using CodeInsight.Library;
using CodeInsight.Web.Common.Charts;
using NodaTime;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestIndexViewModel : ChartsViewModel
    {        
        public PullRequestIndexViewModel(FiniteInterval interval, IReadOnlyList<Domain.PullRequest> pullRequests, IReadOnlyList<Chart> charts) : base(charts)
        {
            Interval = interval;
            PullRequests = pullRequests;
        }
        
        public FiniteInterval Interval { get; }
        
        public IReadOnlyList<Domain.PullRequest> PullRequests { get; }
    }
}
using System.Collections.Generic;
using CodeInsight.Library;
using CodeInsight.Library.Types;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common.Charts;
using NodaTime;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestIndexViewModel : ChartsViewModel
    {        
        public PullRequestIndexViewModel(
            IntervalStatisticsConfiguration configuration,
            IReadOnlyList<Domain.PullRequest.PullRequest> pullRequests,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Configuration = configuration;
            PullRequests = pullRequests;
        }

        public IntervalStatisticsConfiguration Configuration { get; }
        
        public IReadOnlyList<Domain.PullRequest.PullRequest> PullRequests { get; }
    }
}
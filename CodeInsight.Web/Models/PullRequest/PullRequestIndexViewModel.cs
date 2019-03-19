using System.Collections.Generic;
using CodeInsight.Library;
using CodeInsight.Library.Types;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common.Charts;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Web.Models.PullRequest
{
    public sealed class PullRequestIndexViewModel : ChartsViewModel
    {        
        public PullRequestIndexViewModel(
            IntervalStatisticsConfiguration configuration,
            IReadOnlyList<Domain.PullRequest.PullRequest> pullRequests,
            IOption<string> error,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Configuration = configuration;
            PullRequests = pullRequests;
            Error = error;
        }

        public IntervalStatisticsConfiguration Configuration { get; }
        
        public IReadOnlyList<Domain.PullRequest.PullRequest> PullRequests { get; }
        
        public IOption<string> Error { get; }
    }
}
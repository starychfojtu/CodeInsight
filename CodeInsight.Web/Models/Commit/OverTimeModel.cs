using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.PullRequests;
using CodeInsight.Web.Common.Charts;
using FuncSharp;

namespace CodeInsight.Web.Models.Commit
{
    public class OverTimeModel : ChartsViewModel
    {
        public OverTimeModel (
            IntervalStatisticsConfiguration configuration,
            IOption<string> error,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Configuration = configuration;
            Error = error;
        }

        public IntervalStatisticsConfiguration Configuration { get; }

        public IOption<string> Error { get; }
    }
}

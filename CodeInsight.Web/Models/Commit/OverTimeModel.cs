using System.Collections.Generic;
using CodeInsight.Commits;
using CodeInsight.Web.Common.Charts;
using FuncSharp;

namespace CodeInsight.Web.Models.Commit
{
    public class OverTimeModel : ChartsViewModel
    {
        public OverTimeModel (
            OTStatsConfig configuration,
            IOption<string> error,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Configuration = configuration;
            Error = error;
        }

        public OTStatsConfig Configuration { get; }

        public IOption<string> Error { get; }
    }
}

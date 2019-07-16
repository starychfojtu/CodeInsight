using System.Collections.Generic;
using CodeInsight.Commits;
using CodeInsight.Library.DatePicker;
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

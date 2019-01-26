using ChartJSCore.Models;

namespace CodeInsight.Web.Models
{
    public class PullRequestIndexViewModel
    {
        public PullRequestIndexViewModel(Chart chart)
        {
            Chart = chart;
        }

        public Chart Chart { get; }
    }
}
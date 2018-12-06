using CodeInsight.PullRequests;

namespace CodeInsight.Web.Models
{
    public class PullRequestIndexViewModel
    {
        public PullRequestIndexViewModel(RepositoryDayStatistics statistics)
        {
            Statistics = statistics;
        }

        public RepositoryDayStatistics Statistics { get; }
    }
}
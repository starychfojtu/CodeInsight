using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeInsight.Domain;
using CodeInsight.Library;
using FuncSharp;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public sealed class RepositoryDayStatistics
    {
        private readonly DataCube1<LocalDate, IImmutableSet<PullRequest>> data;

        public RepositoryDayStatistics(DataCube1<LocalDate, IImmutableSet<PullRequest>> data, ZonedDateInterval interval)
        {
            this.data = data;
            Interval = interval;
        }
        
        public ZonedDateInterval Interval { get; }
        
        public IOption<RepositoryStatistics> Get(LocalDate date) =>
            data.Get(date).Map(ToStatistics);

        public DataCube2<LocalDate, AccountId, RepositoryStatistics> GetByAuthorsForInterval()
        {
            var stats = new DataCube2<LocalDate, AccountId, RepositoryStatistics>();
            foreach (var date in Interval.DateInterval)
            {
                foreach (var pr in data.Get(date).Flatten())
                {
                    stats.SetOrElseUpdate(
                        date,
                        pr.AuthorId,
                        RepositoryStatistics.FromPullRequest(Interval.End.ToInstant(), pr),
                        RepositoryStatistics.Append
                    );
                }
            }
            return stats;
        }

        private RepositoryStatistics ToStatistics(IEnumerable<PullRequest> pullRequests) =>
            pullRequests
                .Select(p => RepositoryStatistics.FromPullRequest(Interval.End.ToInstant(), p))
                .Aggregate(RepositoryStatistics.Append);
        
//        private DataCube1<LocalMonth, IImmutableSet<PullRequest>> TransformedToMonths =>
//            data.Transform(p => Position1.Create(LocalMonth.Create(p.ProductValue1)), (h1, h2) => h1.Union(h2));
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library;
using FuncSharp;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public sealed class RepositoryDayStatistics
    {
        private readonly DataCube1<LocalDate, IImmutableSet<PullRequest>> data;

        public RepositoryDayStatistics(DataCube1<LocalDate, IImmutableSet<PullRequest>> data, RepositoryDayStatisticsConfiguration configuration)
        {
            this.data = data;
            Configuration = configuration;
        }
        
        public RepositoryDayStatisticsConfiguration Configuration { get; }
        public ZonedDateInterval Interval => Configuration.Interval;
        
        public IOption<RepositoryStatistics> Get(LocalDate date) =>
            data.Get(date).Map(ToStatistics);

        private RepositoryStatistics ToStatistics(IEnumerable<PullRequest> pullRequests) =>
            pullRequests
                .Select(p => RepositoryStatistics.FromPullRequest(Configuration.CalculateAt, p))
                .Aggregate(RepositoryStatistics.Append);
        
        // TODO: To be used.
        private DataCube1<LocalMonth, IImmutableSet<PullRequest>> TransformedToMonths =>
            data.Transform(p => Position1.Create(LocalMonth.Create(p.ProductValue1)), (h1, h2) => h1.Union(h2));
    }
}
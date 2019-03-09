using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using CodeInsight.Library;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public sealed class IntervalStatistics
    {
        private readonly DataCube1<LocalDate, IImmutableSet<PullRequest>> data;

        public IntervalStatistics(
            DataCube1<LocalDate, IImmutableSet<PullRequest>> data,
            IntervalStatisticsConfiguration configuration)
        {
            this.data = data;
            Configuration = configuration;
        }
        
        public IntervalStatisticsConfiguration Configuration { get; }
        public ZonedDateInterval Interval => Configuration.Interval;
        
        public IOption<Statistics> Get(LocalDate date) =>
            data.Get(date).Map(ToStatistics);

        private Statistics ToStatistics(IEnumerable<PullRequest> pullRequests) =>
            pullRequests
                .Select(p => Statistics.FromPullRequest(Configuration.CalculateAt, p))
                .Aggregate(Statistics.Append);
    }
}
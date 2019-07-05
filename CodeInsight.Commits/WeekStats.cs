using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeInsight.Domain.Commit;
using CodeInsight.Library.Extensions;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Commits
{
    public sealed class WeekStats
    {
        private readonly DataCube1<LocalDate, IImmutableSet<Commit>> Data;

        public WeekStats(
            DataCube1<LocalDate, IImmutableSet<Commit>> data
            )
        {
            this.Data = data;
        }

        public DataCube1<LocalDate, T> Map<T>(Func<OverTimeStats, T> project) =>
            Data.Map(ToStats).Map(project);

        public IOption<OverTimeStats> Get(LocalDate date) =>
            Data.Get(date).Map(ToStats);

        private OverTimeStats ToStats(IEnumerable<Commit> commits) =>
            commits
                .Select(OverTimeStats.FromCommits)
                .Aggregate(OverTimeStats.Combine);
    }
}

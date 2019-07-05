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
    public sealed class OverTimeStats
    {
        private readonly DataCube1<LocalDate, IImmutableSet<Commit>> Data;

        public OverTimeStats(
            DataCube1<LocalDate, IImmutableSet<Commit>> data
            )
        {
            this.Data = data;
        }

        public DataCube1<LocalDate, T> Map<T>(Func<WeekStats, T> project) =>
            Data.Map(ToStats).Map(project);

        public IOption<WeekStats> Get(LocalDate date) =>
            Data.Get(date).Map(ToStats);

        private WeekStats ToStats(IEnumerable<Commit> commits) =>
            commits
                .Select(WeekStats.FromCommits)
                .Aggregate(WeekStats.Combine);
    }
}

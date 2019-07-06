using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Commit;
using NodaTime;

namespace CodeInsight.Commits
{
    public class DayStats
    {
        public Instant Day { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }

        public DayStats(Instant day, uint additions, uint deletions, uint commitCount)
        {
            Day = day;
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public static DayStats FromCommits(Commit commit)
        {
            return new DayStats(commit.CommittedAt, commit.Additions, commit.Deletions, 1);
        }

        public static DayStats Combine(DayStats a, DayStats b)
        {
            return new DayStats(
                a.Day,
                a.Additions + b.Additions,
                a.Deletions + b.Deletions,
                a.CommitCount + b.CommitCount);
        }
    }
}

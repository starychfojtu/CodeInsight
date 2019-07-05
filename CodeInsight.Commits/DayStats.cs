using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Commit;

namespace CodeInsight.Commits
{
    class DayStats
    {
        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }

        public DayStats(uint additions, uint deletions, uint commitCount)
        {
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public static WeekStats FromCommits(Commit commit)
        {
            return new WeekStats(commit.Additions, commit.Deletions, 1);
        }

        public static DayStats Combine(DayStats a, DayStats b)
        {
            return new DayStats(
                a.Additions + b.Additions,
                a.Deletions + b.Deletions,
                a.CommitCount + b.CommitCount);
        }
    }
}

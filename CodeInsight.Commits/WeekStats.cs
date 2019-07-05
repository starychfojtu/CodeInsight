using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Commit;
using NodaTime;

namespace CodeInsight.Commits
{
    public class WeekStats
    {
        public Period Period { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }

        public WeekStats(uint additions, uint deletions, uint commitCount)
        {
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public static WeekStats FromCommits(Commit commit)
        {
            return new WeekStats(commit.Additions, commit.Deletions, 1);
        }

        public static WeekStats Combine(WeekStats a, WeekStats b)
        {
            return new WeekStats(
                a.Additions+b.Additions,
                a.Deletions+b.Deletions,
                a.CommitCount+b.CommitCount);
        }
    }
}

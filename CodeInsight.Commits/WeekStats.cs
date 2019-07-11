using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Commit;
using NodaTime;

namespace CodeInsight.Commits
{
    public class WeekStats
    {
        public Instant FirstDay { get; }

        public uint Duration { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }

        public WeekStats(Instant firstDay, uint duration, uint additions, uint deletions, uint commitCount)
        {
            FirstDay = firstDay;
            Duration = duration;
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public static WeekStats FromCommits(Commit commit)
        {
            return new WeekStats(commit.CommittedAt, 1, commit.Additions, commit.Deletions, 1);
        }

        //TODO: Delete
        public static WeekStats Combine(WeekStats a, WeekStats b)
        {
            return new WeekStats(
                a.FirstDay,
                a.Duration,
                a.Additions+b.Additions,
                a.Deletions+b.Deletions,
                a.CommitCount+b.CommitCount);
        }
    }
}

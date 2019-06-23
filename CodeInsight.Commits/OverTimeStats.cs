using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Commit;

namespace CodeInsight.Commits
{
    public class OverTimeStats
    {
        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }

        public OverTimeStats(uint additions, uint deletions, uint commitCount)
        {
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public static OverTimeStats FromCommits(Commit commit)
        {
            return new OverTimeStats(commit.Additions, commit.Deletions, 1);
        }

        public static OverTimeStats Combine(OverTimeStats a, OverTimeStats b)
        {
            return new OverTimeStats(
                a.Additions+b.Additions,
                a.Deletions+b.Deletions,
                a.CommitCount+b.CommitCount);
        }
    }
}

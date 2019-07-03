using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Domain.Issue;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Commits
{
    //UNUSED
    public class PerTaskStats
    {
        public Instant LastCommit { get; }

        public NonEmptyString Id { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CodeChangeDiff => Additions - Deletions;

        public uint ChangedFilesCount { get; }

        public uint AuthorsCount { get; }

        public PerTaskStats(
            Instant lastCommit, 
            NonEmptyString id, 
            uint additions, 
            uint deletions, 
            uint changedFilesCount,
            uint authorsCount)
        {
            LastCommit = lastCommit;
            Id = id;
            Additions = additions;
            Deletions = deletions;
            ChangedFilesCount = changedFilesCount;
            AuthorsCount = authorsCount;
        }

        public static PerTaskStats FromIssue(Issue issue)
        {
            return new PerTaskStats(
                issue.LastCommitAt,
                issue.Id,
                issue.Additions,
                issue.Deletions,
                issue.ChangedFilesCount,
                issue.AuthorsCount
                );
        }
    }
}

using System;
using CodeInsight.Library.Types;
using NodaTime.Extensions;

namespace CodeInsight.Data.Issue
{
    public sealed class Issue
    {
        //TO DO: Solve the numbers of files changed in an issue
        public string Id { get; private set; }

        public string RepositoryId { get; private set; }

        public int Additions { get; private set; }

        public int Deletions { get; private set; }

        public DateTimeOffset LastCommitAt { get; private set; }

        public DateTimeOffset LastUpdateAt { get; private set; }

        public int ChangedFilesCount { get; private set; }

        public int AuthorsCount { get; private set; }

        public Issue(
            string id, 
            string repositoryId, 
            int additions, 
            int deletions,
            DateTimeOffset lastCommitAt, 
            DateTimeOffset lastUpdateAt, 
            int changedFilesCount, 
            int authorsCount)
        {
            Id = id;
            RepositoryId = repositoryId;
            Additions = additions;
            Deletions = deletions;
            LastCommitAt = lastCommitAt;
            LastUpdateAt = lastUpdateAt;
            ChangedFilesCount = changedFilesCount;
            AuthorsCount = authorsCount;
        }

        public static Issue FromDomain(Domain.Issue.Issue issue)
        {
            return new Issue(
                issue.Id,
                issue.RepositoryId,
                (int)issue.Additions,
                (int)issue.Deletions,
                issue.LastCommitAt.ToDateTimeOffset(),
                issue.LastUpdateAt.ToDateTimeOffset(),
                (int)issue.ChangedFilesCount,
                (int)issue.AuthorsCount
                );
        }

        public static Domain.Issue.Issue ToDomain(Issue issue)
        {
            return new Domain.Issue.Issue(
                NonEmptyString.Create(issue.Id).Get(),
                NonEmptyString.Create(issue.RepositoryId).Get(),
                (uint)issue.Additions,
                (uint)issue.Deletions,
                issue.LastCommitAt.ToInstant(),
                issue.LastUpdateAt.ToInstant(),
                (uint)issue.ChangedFilesCount,
                (uint)issue.AuthorsCount
                );
        }
    }
}

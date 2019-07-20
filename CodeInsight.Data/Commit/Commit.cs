using System;
using CodeInsight.Library.Types;
using NodaTime.Extensions;

namespace CodeInsight.Data.Commit
{
    public sealed class Commit
    {
        public string Id { get; private set; }

        public string RepositoryId { get; private set; }
        
        public string AuthorName { get; private set; }

        public int Additions { get; private set; }

        public int Deletions { get; private set; }

        public DateTimeOffset CommittedAt { get; private set; }

        //TEMP - might be useful for task/issue<->commit connection
        public string CommitMsg { get; private set; }

        public Commit(
            string id, 
            string repositoryId, 
            string authorName, 
            int additions, 
            int deletions, 
            DateTimeOffset committedAt, 
            string commitMsg)
        {
            Id = id;
            RepositoryId = repositoryId;
            AuthorName = authorName;
            Additions = additions;
            Deletions = deletions;
            CommittedAt = committedAt;
            CommitMsg = commitMsg;
        }

        public static Commit FromDomain(Domain.Commit.Commit commit)
        {
            return new Commit(
                commit.Id,
                commit.RepositoryId,
                commit.AuthorName,
                (int)commit.Additions,
                (int)commit.Deletions,
                commit.CommittedAt.ToDateTimeOffset(),
                commit.CommitMsg
                );
        }
        public static Domain.Commit.Commit ToDomain(Commit commit)
        {
            return new Domain.Commit.Commit(
                NonEmptyString.Create(commit.Id).Get(),
                NonEmptyString.Create(commit.RepositoryId).Get(),
                NonEmptyString.Create(commit.AuthorName).Get(),
                (uint) commit.Additions,
                (uint) commit.Deletions,
                commit.CommittedAt.ToInstant(),
                NonEmptyString.Create(commit.CommitMsg).Get()
                );
        }
    }
}

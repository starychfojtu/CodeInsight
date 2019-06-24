using System;
using CodeInsight.Domain;
using CodeInsight.Library.Types;
using NodaTime.Extensions;

namespace CodeInsight.Data.Commit
{
    public sealed class Commit
    {
        public string Id { get; private set; }

        public string RepositoryId { get; private set; }
        
        public string AuthorName { get; private set; }

        public string AuthorId { get; private set; }

        public int Additions { get; private set; }

        public int Deletions { get; private set; }

        public DateTimeOffset CommitedAt { get; private set; }

        //TEMP - might be useful for task<->commit connection
        public string Comment { get; private set; }

        public Commit(
            string id, 
            string repositoryId, 
            string authorName, 
            string authorId, 
            int additions, 
            int deletions, 
            DateTimeOffset commitedAt, 
            string comment)
        {
            Id = id;
            RepositoryId = repositoryId;
            AuthorName = authorName;
            AuthorId = authorId;
            Additions = additions;
            Deletions = deletions;
            CommitedAt = commitedAt;
            Comment = comment;
        }

        public static Commit FromDomain(Domain.Commit.Commit commit)
        {
            return new Commit(
                commit.Id,
                commit.RepositoryId,
                commit.AuthorName,
                commit.AuthorId,
                (int)commit.Additions,
                (int)commit.Deletions,
                commit.CommitedAt.ToDateTimeOffset(),
                commit.Comment
                );
        }
        public static Domain.Commit.Commit ToDomain(Commit commit)
        {
            return new Domain.Commit.Commit(
                NonEmptyString.Create(commit.Id).Get(),
                NonEmptyString.Create(commit.RepositoryId).Get(),
                NonEmptyString.Create(commit.AuthorName).Get(),
                NonEmptyString.Create(commit.AuthorId).Get(),
                (uint) commit.Additions,
                (uint) commit.Deletions,
                commit.CommitedAt.ToInstant(),
                NonEmptyString.Create(commit.Comment).Get()
                );
        }
    }
}

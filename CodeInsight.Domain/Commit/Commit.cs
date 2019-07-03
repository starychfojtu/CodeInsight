using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Domain.Commit
{
    public sealed class Commit
    {
        public NonEmptyString Id { get; private set; }

        public NonEmptyString RepositoryId { get; private set; }

        public NonEmptyString AuthorName { get; private set; }

        public NonEmptyString AuthorId { get; private set; }

        public uint Additions { get; private set; }

        public uint Deletions { get; private set; }

        public Instant CommittedAt { get; private set; }

        //TEMP - might be useful for task<->commit connection
        public NonEmptyString Comment { get; private set; }

        public Commit(
            NonEmptyString id, 
            NonEmptyString repositoryId, 
            NonEmptyString authorName,
            NonEmptyString authorId, 
            uint additions, 
            uint deletions, 
            Instant committedAt,
            NonEmptyString comment)
        {
            Id = id;
            RepositoryId = repositoryId;
            AuthorName = authorName;
            AuthorId = authorId;
            Additions = additions;
            Deletions = deletions;
            CommittedAt = committedAt;
            Comment = comment;
        }

        private bool Equals(Commit other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Commit other && Equals(other);
        }

        public override int GetHashCode()
        { 
            return Id.GetHashCode();
        }
    }
}

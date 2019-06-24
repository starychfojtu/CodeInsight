using System;
using System.Collections.Generic;
using System.Text;
using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Domain.Issue
{
    public sealed class Issue
    {
        public NonEmptyString Id { get; private set; }

        public NonEmptyString RepositoryId { get; private set; }

        public uint Additions { get; private set; }

        public uint Deletions { get; private set; }

        public Instant LastCommitAt { get; private set; }

        public Instant LastUpdateAt { get; private set; }

        public uint ChangedFilesCount { get; private set; }

        public uint AuthorsCount { get; private set; }

        public Issue(
            NonEmptyString id, 
            NonEmptyString repositoryId, 
            uint additions, 
            uint deletions, 
            Instant lastCommitAt, 
            Instant lastUpdateAt, 
            uint changedFilesCount, 
            uint authorsCount)
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

        private bool Equals(Issue other)
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

            return obj is Issue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

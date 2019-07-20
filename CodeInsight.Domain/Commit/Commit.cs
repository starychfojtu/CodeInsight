using CodeInsight.Library.Types;
using NodaTime;

namespace CodeInsight.Domain.Commit
{
    public sealed class Commit
    {
        public Commit(
            NonEmptyString id,
            NonEmptyString repositoryId,
            NonEmptyString authorName,
            uint additions,
            uint deletions,
            Instant committedAt,
            NonEmptyString commitMsg)
        {
            Id = id;
            RepositoryId = repositoryId;
            AuthorName = authorName;
            Additions = additions;
            Deletions = deletions;
            CommittedAt = committedAt;
            CommitMsg = commitMsg;
        }

        public NonEmptyString Id { get; private set; }

        public NonEmptyString RepositoryId { get; private set; }

        public NonEmptyString AuthorName { get; private set; }


        public uint Additions { get; private set; }

        public uint Deletions { get; private set; }

        public Instant CommittedAt { get; private set; }

        //TEMP - might be useful for task<->commit connection
        public NonEmptyString CommitMsg { get; private set; }
        
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

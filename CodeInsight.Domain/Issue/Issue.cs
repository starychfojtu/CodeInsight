using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Domain.Issue
{
    public sealed class Issue
    {
        public Issue(
            uint id,
            NonEmptyString title, 
            NonEmptyString repositoryId, 
            IOption<Instant> closedAt, 
            Instant createdAt,
            Instant lastUpdateAt, 
            uint commentCount)
        {
            Id = id;
            Title = title;
            RepositoryId = repositoryId;
            ClosedAt = closedAt;
            CreatedAt = createdAt;
            LastUpdateAt = lastUpdateAt;
            CommentCount = commentCount;
        }

        public uint Id { get; private set; }

        public NonEmptyString Title { get; private set; }

        public NonEmptyString RepositoryId { get; private set; }

        public IOption<Instant> ClosedAt { get; private set; }

        public Instant CreatedAt { get; private set; }

        public Instant LastUpdateAt { get; private set; }

        public uint CommentCount { get; private set; }


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

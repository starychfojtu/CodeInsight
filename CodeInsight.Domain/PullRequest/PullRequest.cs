using CodeInsight.Library;
using FuncSharp;
using NodaTime;

namespace CodeInsight.Domain.PullRequest
{
    public sealed class PullRequest
    {
        public PullRequest(
            NonEmptyString id,
            NonEmptyString repositoryId,
            NonEmptyString title,
            AccountId authorId,
            uint deletions,
            uint additions,
            Instant createdAt,
            Instant updatedAt,
            IOption<Instant> mergedAt,
            IOption<Instant> closedAt, 
            uint commentCount)
        {
            Id = id;
            RepositoryId = repositoryId;
            Title = title;
            AuthorId = authorId;
            Deletions = deletions;
            Additions = additions;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            MergedAt = mergedAt;
            ClosedAt = closedAt;
            CommentCount = commentCount;
        }

        public NonEmptyString Id { get; }
        
        public NonEmptyString RepositoryId { get; }

        public NonEmptyString Title { get; }
        
        public AccountId AuthorId { get; }

        public uint Deletions { get; }
        
        public uint Additions { get; }
        
        public Instant CreatedAt { get; }
        
        public Instant UpdatedAt { get; }

        public IOption<Instant> MergedAt { get; }
        
        public IOption<Instant> ClosedAt { get; }
        
        public uint CommentCount { get; }

        public IOption<Instant> End =>
            MergedAt.Match(
                m => MergedAt,
                _ => ClosedAt
            );
        
        public Interval Interval => 
            new Interval(CreatedAt, End.ToNullable());

        public IOption<Duration> Lifetime =>
            Interval.HasEnd ? Prelude.Some(Interval.Duration) : Prelude.None<Duration>();

        private bool Equals(PullRequest other) =>
            string.Equals(Id, other.Id);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PullRequest other && Equals(other);
        }

        public override int GetHashCode() =>
            Id.GetHashCode();
    }
}
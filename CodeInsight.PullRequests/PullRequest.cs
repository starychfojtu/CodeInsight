using CodeInsight.Domain;
using CodeInsight.Library;
using FuncSharp;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.PullRequests
{
    public sealed class PullRequest
    {
        public PullRequest(
            NonEmptyString id,
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

        public IOption<Duration> Lifetime =>
            MergedAt.Match(
                m => Some(m - CreatedAt),
                _ => ClosedAt.Map(c => c - CreatedAt)
            );

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
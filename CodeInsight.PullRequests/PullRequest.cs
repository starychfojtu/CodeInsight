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
            AccountId authorId,
            uint deletions,
            uint additions,
            Instant createdAt,
            IOption<Instant> mergedAt,
            IOption<Instant> closedAt)
        {
            Id = id;
            AuthorId = authorId;
            Deletions = deletions;
            Additions = additions;
            CreatedAt = createdAt;
            MergedAt = mergedAt;
            ClosedAt = closedAt;
        }

        public NonEmptyString Id { get; }
        
        public AccountId AuthorId { get; }

        public uint Deletions { get; }
        
        public uint Additions { get; }
        
        public Instant CreatedAt { get; }
        
        public IOption<Instant> MergedAt { get; }
        
        public IOption<Instant> ClosedAt { get; }

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
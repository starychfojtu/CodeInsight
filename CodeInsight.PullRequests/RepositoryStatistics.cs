using FuncSharp;
using NodaTime;

namespace CodeInsight.PullRequests
{
    public class RepositoryStatistics
    {
        public RepositoryStatistics(
            uint deletions,
            uint additions,
            Duration lifeTime,
            uint pullRequestCount)
        {
            Deletions = deletions;
            Additions = additions;
            Lifetime = lifeTime;
            PullRequestCount = pullRequestCount;
        }
        
        public uint Deletions { get; }
        
        public uint Additions { get; }
        
        public Duration Lifetime { get; }
        
        public uint PullRequestCount { get; }

        public double AverageDeletions => Deletions / (double)PullRequestCount;
        public double AverageAdditions => Additions / (double)PullRequestCount;
        public Duration AverageLifeTime => Lifetime / PullRequestCount;
        public Duration WightedAverageLifeTime => Lifetime * (Additions + Deletions) / PullRequestCount;
        
        public static RepositoryStatistics FromPullRequest(Instant nowUtc, PullRequest pr) =>
            new RepositoryStatistics(pr.Deletions, pr.Additions, pr.Lifetime.GetOrElse(nowUtc - pr.CreatedAt), 1);
        
        public static RepositoryStatistics Append(RepositoryStatistics a, RepositoryStatistics b) =>
            new RepositoryStatistics(a.Deletions + b.Deletions, a.Additions + b.Additions, a.Lifetime + b.Lifetime, a.PullRequestCount + b.PullRequestCount);
    }
}
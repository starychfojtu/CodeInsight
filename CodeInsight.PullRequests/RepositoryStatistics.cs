using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using FuncSharp;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.PullRequests
{
    public class RepositoryStatistics
    {
        public RepositoryStatistics(
            uint deletions,
            uint additions,
            Duration lifeTime,
            uint pullRequestCount,
            Duration changesWeightedLifetime)
        {
            Deletions = deletions;
            Additions = additions;
            Lifetime = lifeTime;
            PullRequestCount = pullRequestCount;
            ChangesWeightedLifetime = changesWeightedLifetime;
        }
        
        public uint Deletions { get; }
        
        public uint Additions { get; }
        
        public Duration Lifetime { get; }
        
        public uint PullRequestCount { get; }
        
        public Duration ChangesWeightedLifetime { get; }

        public uint Changes => Additions + Deletions;
        public double AverageDeletions => Deletions / (double)PullRequestCount;
        public double AverageAdditions => Additions / (double)PullRequestCount;
        public Duration AverageLifeTime => Lifetime / PullRequestCount;
        public IOption<Duration> ChangesWeightedAverageLifeTime => 
            Changes == 0 ? None<Duration>() : Some(ChangesWeightedLifetime / Changes);

        public static RepositoryStatistics FromPullRequest(Instant nowUtc, PullRequest pr)
        {
            var lifeTime = pr.Lifetime.GetOrElse(nowUtc - pr.CreatedAt);
            return new RepositoryStatistics(pr.Deletions, pr.Additions, lifeTime, 1, lifeTime * (pr.Additions + pr.Deletions));
        }
            
        public static RepositoryStatistics Append(RepositoryStatistics a, RepositoryStatistics b) =>
            new RepositoryStatistics(
                a.Deletions + b.Deletions,
                a.Additions + b.Additions,
                a.Lifetime + b.Lifetime,
                a.PullRequestCount + b.PullRequestCount,
                a.ChangesWeightedLifetime + b.ChangesWeightedLifetime
            );
    }
}
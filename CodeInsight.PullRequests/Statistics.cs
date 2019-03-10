using CodeInsight.Domain;
using CodeInsight.Domain.PullRequest;
using FuncSharp;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.PullRequests
{
    public class Statistics
    {
        public Statistics(
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
        

        public uint Changes => Additions + Deletions;
        public double AverageDeletions => Deletions / (double)PullRequestCount;
        public double AverageAdditions => Additions / (double)PullRequestCount;
        public Duration AverageLifeTime => Lifetime / PullRequestCount;
        public Efficiency AverageEfficiency => Efficiency.Create(Changes, Lifetime);

        public static Statistics FromPullRequest(Instant nowUtc, PullRequest pr) =>
             new Statistics(pr.Deletions, pr.Additions, pr.Lifetime.GetOrElse(nowUtc - pr.CreatedAt), 1);
            
        public static Statistics Append(Statistics a, Statistics b) =>
            new Statistics(
                a.Deletions + b.Deletions,
                a.Additions + b.Additions,
                a.Lifetime + b.Lifetime,
                a.PullRequestCount + b.PullRequestCount
            );
    }
}
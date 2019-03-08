using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Library;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.PullRequests
{
    public sealed class SampleRepository : IPullRequestRepository
    {
        public Task<IEnumerable<PullRequest>> GetAllOpenOrClosedAfter(Instant minCreatedAt)
        {
            var createdAt = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            var pr1 = new PullRequest(
                NonEmptyString.Create("1").Get(),
                NonEmptyString.Create("1").Get(),
                NonEmptyString.Create("1").Get(),
                new AccountId("A"),
                deletions: 10,
                additions: 20,
                createdAt: createdAt,
                updatedAt: createdAt,
                mergedAt: Some(createdAt.Plus(Duration.FromDays(5))),
                closedAt: None<Instant>(),
                commentCount: 20
            );
            var pr2 = new PullRequest(
                NonEmptyString.Create("2").Get(),
                NonEmptyString.Create("2").Get(),
                NonEmptyString.Create("2").Get(),
                new AccountId("A"),
                deletions: 420,
                additions: 140,
                createdAt: createdAt,
                updatedAt: createdAt,
                mergedAt: Some(createdAt.Plus(Duration.FromDays(9))),
                closedAt: None<Instant>(),
                commentCount: 10
            );
            var pr3 = new PullRequest(
                NonEmptyString.Create("3").Get(),
                NonEmptyString.Create("3").Get(),
                NonEmptyString.Create("3").Get(),
                new AccountId("B"),
                deletions: 530,
                additions: 260,
                createdAt: createdAt,
                updatedAt: createdAt,
                mergedAt: None<Instant>(),
                closedAt: None<Instant>(),
                commentCount: 6
            );
            return (new [] { pr1, pr2, pr3 }).Async<PullRequest[], IEnumerable<PullRequest>>();
        }
    }
}
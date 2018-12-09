using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Library;
using NodaTime;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.PullRequests
{
    public sealed class SampleRepository : IPullRequestRepository
    {
        public Task<IEnumerable<PullRequest>> GetAll()
        {
            var createdAt = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(10));
            var pr1 = new PullRequest(NonEmptyString.Create("1").Get(), new AccountId("A"),  10, 20, createdAt, Some(createdAt.Plus(Duration.FromDays(5))), None<Instant>());
            var pr2 = new PullRequest(NonEmptyString.Create("2").Get(), new AccountId("A"), 420, 140, createdAt, Some(createdAt.Plus(Duration.FromDays(9))), None<Instant>());
            var pr3 = new PullRequest(NonEmptyString.Create("3").Get(), new AccountId("B"), 530, 260, createdAt, None<Instant>(), None<Instant>());
            return ((IEnumerable<PullRequest>)new [] { pr1, pr2, pr3 }).Async();
        }
    }
}
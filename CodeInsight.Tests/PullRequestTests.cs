using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using NodaTime;
using Xunit;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Tests
{
    public class PullRequestTests
    {
        [Fact]
        public void RepositoryDayStatistics()
        {
            var start = new LocalDate(2018, 11, 20);
            var end = new LocalDate(2018, 11, 30);
            var dayBeforeEnd = end.Minus(Period.FromDays(1));
            var createdAt = start.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
            var aId = new AccountId("A");
            var bId = new AccountId("B");
            var pr1 = new PullRequest(NonEmptyString.Create("1").Get(), NonEmptyString.Create("1").Get(), NonEmptyString.Create("1").Get(), aId, 10, 20, createdAt, createdAt, Some(createdAt.Plus(Duration.FromDays(5))), None<Instant>(), 0);
            var pr2 = new PullRequest(NonEmptyString.Create("2").Get(), NonEmptyString.Create("2").Get(), NonEmptyString.Create("1").Get(), bId, 20, 40, createdAt, createdAt, Some(createdAt.Plus(Duration.FromDays(9))), None<Instant>(), 0);
            var pr3 = new PullRequest(NonEmptyString.Create("3").Get(), NonEmptyString.Create("3").Get(), NonEmptyString.Create("1").Get(), aId, 100, 60, createdAt,createdAt,  None<Instant>(), None<Instant>(), 0);
            var prs = new [] { pr1, pr2, pr3 };

            var interval = new ZonedDateInterval(
                new DateInterval(start, end),
                DateTimeZone.Utc
            );

            var configuration = new RepositoryDayStatisticsConfiguration(interval, interval.End.ToInstant());
            var statistics = RepositoryStatisticsCalculator.Calculate(prs, configuration);
            
            var startStats = statistics.Get(start).Get();
            var dayBeforeEndStats = statistics.Get(dayBeforeEnd).Get();
            var endStats = statistics.Get(end).Get();
            
            Assert.Equal(8, startStats.AverageLifeTime.Days);
            Assert.Equal(9, dayBeforeEndStats.AverageLifeTime.Days);
            Assert.Equal(10, endStats.AverageLifeTime.Days);
            
            Assert.Equal(9, startStats.ChangesWeightedAverageLifeTime.Get().Days);
            Assert.Equal(9, dayBeforeEndStats.ChangesWeightedAverageLifeTime.Get().Days);
            Assert.Equal(10, endStats.ChangesWeightedAverageLifeTime.Get().Days);
        }
    }
}
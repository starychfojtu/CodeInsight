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
        public void GetRepositoryStatistics()
        {
            var start = new LocalDate(2018, 11, 20);
            var end = new LocalDate(2018, 11, 30);
            var dayBeforeEnd = end.Minus(Period.FromDays(1));
            var createdAt = start.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
            var pr1 = new PullRequest(NonEmptyString.Create("1").Get(), 10, 20, createdAt, Some(createdAt.Plus(Duration.FromDays(5))), None<Instant>());
            var pr2 = new PullRequest(NonEmptyString.Create("2").Get(), 20, 40, createdAt, Some(createdAt.Plus(Duration.FromDays(9))), None<Instant>());
            var pr3 = new PullRequest(NonEmptyString.Create("3").Get(), 30, 60, createdAt, None<Instant>(), None<Instant>());
            var prs = new [] { pr1, pr2, pr3 };

            var interval = new ZonedDateInterval(
                new DateInterval(start, end),
                DateTimeZone.Utc
            );
            
            var statistics = RepositoryStatisticsCalculator.Calculate(prs, interval);
            
            Assert.Equal(8, statistics.Get(start).Get().AverageLifeTime.Days);
            Assert.Equal(9, statistics.Get(dayBeforeEnd).Get().AverageLifeTime.Days);
            Assert.Equal(10, statistics.Get(end).Get().AverageLifeTime.Days);
        }
    }
}
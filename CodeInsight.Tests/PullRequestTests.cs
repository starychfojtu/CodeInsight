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
            var pr1 = new PullRequest(NonEmptyString.Create("1").Get(), aId, 10, 20, createdAt, Some(createdAt.Plus(Duration.FromDays(5))), None<Instant>());
            var pr2 = new PullRequest(NonEmptyString.Create("2").Get(), bId, 20, 40, createdAt, Some(createdAt.Plus(Duration.FromDays(9))), None<Instant>());
            var pr3 = new PullRequest(NonEmptyString.Create("3").Get(), aId, 100, 60, createdAt, None<Instant>(), None<Instant>());
            var prs = new [] { pr1, pr2, pr3 };

            var interval = new ZonedDateInterval(
                new DateInterval(start, end),
                DateTimeZone.Utc
            );
            
            var statistics = RepositoryStatisticsCalculator.Calculate(prs, interval);
            
            var startStats = statistics.Get(start).Get();
            var dayBeforeEndStats = statistics.Get(dayBeforeEnd).Get();
            var endStats = statistics.Get(end).Get();
            
            Assert.Equal(8, startStats.AverageLifeTime.Days);
            Assert.Equal(9, dayBeforeEndStats.AverageLifeTime.Days);
            Assert.Equal(10, endStats.AverageLifeTime.Days);
            
            Assert.Equal(9, startStats.ChangesWeightedAverageLifeTime.Days);
            Assert.Equal(9, dayBeforeEndStats.ChangesWeightedAverageLifeTime.Days);
            Assert.Equal(10, endStats.ChangesWeightedAverageLifeTime.Days);
            
            var startStatsByAuthor = statistics.GetByAuthor(start).Get();
            var dayBeforeEndStatsByAuthor = statistics.GetByAuthor(dayBeforeEnd).Get();
            var endStatsByAuthor = statistics.GetByAuthor(end).Get();

            var startStatsOfA = startStatsByAuthor.Get(aId).Get();
            var dayBeforeEndStatsOfA = dayBeforeEndStatsByAuthor.Get(aId).Get();
            var endStatsOfA = endStatsByAuthor.Get(aId).Get();
            
            Assert.Equal(7, startStatsOfA.AverageLifeTime.Days);
            Assert.Equal(10, dayBeforeEndStatsOfA.AverageLifeTime.Days);
            Assert.Equal(10, endStatsOfA.AverageLifeTime.Days);
            
            Assert.Equal(9, startStatsOfA.ChangesWeightedAverageLifeTime.Days);
            Assert.Equal(10, dayBeforeEndStatsOfA.ChangesWeightedAverageLifeTime.Days);
            Assert.Equal(10, endStatsOfA.ChangesWeightedAverageLifeTime.Days);
            
            var startStatsOfB = startStatsByAuthor.Get(bId).Get();
            var dayBeforeEndStatsOfB = dayBeforeEndStatsByAuthor.Get(bId).Get();
            var endStatsOfB = endStatsByAuthor.Get(bId);
            
            Assert.True(endStatsOfB.IsEmpty);
            
            Assert.Equal(9, startStatsOfB.AverageLifeTime.Days);
            Assert.Equal(9, dayBeforeEndStatsOfB.AverageLifeTime.Days);
            
            Assert.Equal(9, startStatsOfB.ChangesWeightedAverageLifeTime.Days);
            Assert.Equal(9, dayBeforeEndStatsOfB.ChangesWeightedAverageLifeTime.Days);
           
        }
    }
}
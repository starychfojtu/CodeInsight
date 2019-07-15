using NodaTime;

namespace CodeInsight.Commits
{
    public class DayStats
    {
        public DayStats(LocalDate day, uint additions, uint deletions, uint commitCount)
        {
            Day = day;
            Additions = additions;
            Deletions = deletions;
            CommitCount = commitCount;
        }

        public LocalDate Day { get; }

        public uint Additions { get; }

        public uint Deletions { get; }

        public uint CommitCount { get; }
    }
}

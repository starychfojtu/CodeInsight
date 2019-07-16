using System.Collections.Generic;
using System.Linq;
using CodeInsight.Domain.Commit;

namespace CodeInsight.Commits
{
    public static class AuthorCalculator
    {
        public static AuthorStats PerAuthor(IEnumerable<Commit> commits, string authName)
        {
            var authoredCommits = commits
                .Where(cm => cm.AuthorName == authName)
                .ToList();
            var min = authoredCommits
                .Min(a => a.CommittedAt);
            var max = authoredCommits
                .Max(a => a.CommittedAt);
            var additions = authoredCommits
                .Sum(cm => cm.Additions);
            var deletions = authoredCommits
                .Sum(cm => cm.Deletions);

            return new AuthorStats(
                authorName: authName,
                additions: (uint) additions,
                deletions: (uint) deletions,
                lastCommitAt: max,
                firstCommitAt: min
                );
        }
    }
}

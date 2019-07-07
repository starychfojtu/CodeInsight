using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CodeInsight.Domain.Commit;

namespace CodeInsight.Commits
{
    public static class AuthorCalculator
    {
        public static AuthorStats PerAuthor(IEnumerable<Commit> commits, string authName)
        {
            var min = commits
                .Where(cm => cm.AuthorName == authName)
                .Min(a => a.CommittedAt);
            var max = commits
                .Where(cm => cm.AuthorName == authName)
                .Max(a => a.CommittedAt);
            var additions = commits
                .Where(cm => cm.AuthorName == authName)
                .Select(cm => cm.Additions)
                .Aggregate<uint, uint>(0, (current, entry) => current + entry);
            var deletions = commits
                .Where(cm => cm.AuthorName == authName)
                .Select(cm => cm.Deletions)
                .Aggregate<uint, uint>(0, (current, entry) => current + entry);

            return new AuthorStats(
                authorName: authName,
                additions: additions,
                deletions: deletions,
                lastCommitAt: max,
                firstCommitAt: min
                );
        }
    }
}

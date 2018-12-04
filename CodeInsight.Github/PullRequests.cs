using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Library;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit;
using PullRequest = CodeInsight.PullRequests.PullRequest;
using Repository = CodeInsight.Domain.Repository;

namespace CodeInsight.Github
{
    public static class PullRequests
    {
        public static Reader<IGitHubClient, Task<IEnumerable<PullRequest>>> Get(Repository repo) =>
            client => client.PullRequest
                .GetAllForRepository(repo.Name, repo.OwnerName, new PullRequestRequest { State = ItemStateFilter.All })
                .Map(prs => prs.Select(ToDomain));
        
        private static PullRequest ToDomain(Octokit.PullRequest pr) =>
            new PullRequest(
                NonEmptyString.Create(pr.Number.ToString()).Get(),
                (uint)pr.Deletions,
                (uint)pr.Additions,
                Instant.FromDateTimeOffset(pr.CreatedAt),
                pr.MergedAt.ToOption().Map(Instant.FromDateTimeOffset),
                pr.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset)
            );
    }
}
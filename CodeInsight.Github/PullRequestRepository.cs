using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Domain;
using CodeInsight.Library;
using CodeInsight.PullRequests;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit;
using PullRequest = CodeInsight.PullRequests.PullRequest;
using Repository = Octokit.Repository;

namespace CodeInsight.Github
{
    public sealed class PullRequestRepository: IPullRequestRepository
    {
        private readonly GithubRepositoryClient client;

        public PullRequestRepository(GithubRepositoryClient client)
        {
            this.client = client;
        }

        public Task<IEnumerable<PullRequest>> GetAll() =>
            Get(client.RepositoryId).Execute(client.Api);

        private static Reader<IGitHubClient, Task<IEnumerable<PullRequest>>> Get(long repositoryId) =>
            client => client.PullRequest
                .GetAllForRepository(repositoryId, new PullRequestRequest {State = ItemStateFilter.All})
                .Map(prs => prs.Select(ToDomain));
    
        private static PullRequest ToDomain(Octokit.PullRequest pr) =>
            new PullRequest(
                NonEmptyString.Create(pr.Number.ToString()).Get(),
                new AccountId(pr.User.Id.ToString()),
                (uint) pr.Deletions,
                (uint) pr.Additions,
                Instant.FromDateTimeOffset(pr.CreatedAt),
                pr.MergedAt.ToOption().Map(Instant.FromDateTimeOffset),
                pr.ClosedAt.ToOption().Map(Instant.FromDateTimeOffset)
            );
    }
}
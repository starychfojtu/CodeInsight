using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repository = CodeInsight.Domain.Repository.Repository;
using CodeInsight.Library.Types;
using NodaTime;
using NodaTime.Extensions;
using Octokit;


namespace CodeInsight.Github.Awaits
{
    internal static class GetAllCommits
    {
        internal static async Task<IEnumerable<CommitDto>> AwaitCommits(IConnection connection, Repository repository)
        {
            var github = new GitHubClient(connection);

            var commits = await github.Repository.Commit.GetAll(repository.Owner.Value, repository.Name.Value);

            //TODO: Fix return
            return commits.Select(commit => new CommitDto
                {
                    Id = commit.Sha,
                    RepositoryId = commit.Repository.Id.ToString(),
                    AuthorName = commit.Commit.Author.Name,
                    AuthorId = commit.Author.Id.ToString(),
                    Additions = commit.Stats.Additions,
                    Deletions = commit.Stats.Deletions,
                    CommittedAt = commit.Commit.Author.Date,
                    CommitMsg = commit.Commit.Message
                });
        }
        internal sealed class CommitDto
        {
            public string Id { get; set; }

            public string RepositoryId { get; set; }

            public string AuthorName { get; set; }

            public string AuthorId { get; set; }

            public int Additions { get; set; }

            public int Deletions { get; set; }

            public DateTimeOffset CommittedAt { get; set; }

            public string CommitMsg { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repository = CodeInsight.Domain.Repository.Repository;
using Octokit;


namespace CodeInsight.Github.Awaits
{
    internal static class GetAllCommits
    {
        internal static async Task<IEnumerable<CommitDto>> AwaitCommits(IConnection connection, Repository repository)
        {
            var github = new GitHubClient(connection);

            var repo = repository;

            var commits = await github.Repository.Commit.GetAll(repository.Owner.Value, repository.Name.Value);

            var result = new List<CommitDto>();
            foreach (var commit in commits)
            {
                var details = await github.Repository.Commit.Get(repo.Owner.Value, repo.Name.Value, commit.Sha);
                var id = details.Sha;
                var repId = repo.Id.Value.Value;
                var authName = details.Commit.Author.Name;
                var add = details.Stats.Additions;
                var del = details.Stats.Deletions;
                var com = details.Commit.Author.Date;
                var msg = details.Commit.Message;

                result.Add(new CommitDto
                    {
                        Id = id,
                        RepositoryId = repId,
                        AuthorName = authName,
                        Additions = add,
                        Deletions = del,
                        CommittedAt = com,
                        CommitMsg = msg
                    });
            }

            return result;
        }
        internal sealed class CommitDto
        {
            public string Id { get; set; }

            public string RepositoryId { get; set; }

            public string AuthorName { get; set; }

            public int Additions { get; set; }

            public int Deletions { get; set; }

            public DateTimeOffset CommittedAt { get; set; }

            public string CommitMsg { get; set; }
        }
    }
}

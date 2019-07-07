using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository = CodeInsight.Domain.Repository.Repository;
using Commit = CodeInsight.Domain.Commit.Commit;
using CodeInsight.Library.Types;
using Monad;
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
            
            var commits = await github.Repository.Commit.GetAll(long.Parse(repository.Id.Value));

            return commits.Select(commit => new CommitDto(
                NonEmptyString.Create(commit.Sha).Get(), 
                NonEmptyString.Create(commit.Repository.Id.ToString()).Get(), 
                NonEmptyString.Create(commit.Commit.Author.Name).Get(), 
                NonEmptyString.Create(commit.Author.Id.ToString()).Get(), 
                (uint) commit.Stats.Additions, 
                (uint) commit.Stats.Deletions, 
                commit.Commit.Author.Date.ToInstant(), 
                NonEmptyString.Create(commit.Commit.Message).Get()))
                .ToList();
        }
        internal sealed class CommitDto
        {
            public NonEmptyString Id { get; private set; }

            public NonEmptyString RepositoryId { get; private set; }

            public NonEmptyString AuthorName { get; private set; }

            public NonEmptyString AuthorId { get; private set; }

            public uint Additions { get; private set; }

            public uint Deletions { get; private set; }

            public Instant CommittedAt { get; private set; }

            public NonEmptyString Comment { get; private set; }

            public CommitDto(
                NonEmptyString id,
                NonEmptyString repositoryId,
                NonEmptyString authorName,
                NonEmptyString authorId,
                uint additions,
                uint deletions,
                Instant committedAt,
                NonEmptyString comment)
            {
                Id = id;
                RepositoryId = repositoryId;
                AuthorName = authorName;
                AuthorId = authorId;
                Additions = additions;
                Deletions = deletions;
                CommittedAt = committedAt;
                Comment = comment;
            }
        }
    }
}

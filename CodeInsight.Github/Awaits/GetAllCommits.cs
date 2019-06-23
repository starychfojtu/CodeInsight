using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    //TODO: Add a way of getting the data from github
    internal class GetAllCommits
    {
        //TODO: 
        private async Task<List<CommitDto>> AwaitCommits(IConnection connection, Repository repository)
        {
            var github = new GitHubClient(connection);
            
            var commits = await github.Repository.Commit.GetAll(Int64.Parse(repository.Id.Value));

            var ret = new List<CommitDto>();
            GitHubCommit a = new GitHubCommit();
            
            foreach (var commit in commits)
            {
                var newEntry = new CommitDto(
                    NonEmptyString.Create(commit.Sha).Get(),
                    NonEmptyString.Create(commit.Repository.Id.ToString()).Get(),
                    NonEmptyString.Create(commit.Commit.User.Name).Get(),
                    NonEmptyString.Create(commit.Committer.Id.ToString()).Get(),
                    (uint) commit.Stats.Additions,
                    (uint) commit.Stats.Deletions,
                    commit.Commit.Author.Date.ToInstant(),
                    NonEmptyString.Create(commit.Commit.Message).Get()
                    );
                ret.Add(newEntry);
            }
            return ret;
        }


        internal sealed class CommitDto
        {
            public NonEmptyString Id { get; private set; }

            public NonEmptyString RepositoryId { get; private set; }

            public NonEmptyString AuthorName { get; private set; }

            public NonEmptyString AuthorId { get; private set; }

            public uint Additions { get; private set; }

            public uint Deletions { get; private set; }

            public Instant CommitedAt { get; private set; }

            //TEMP - might be useful for task<->commit connection
            public NonEmptyString Comment { get; private set; }

            public CommitDto(
                NonEmptyString id, 
                NonEmptyString repositoryId, 
                NonEmptyString authorName, 
                NonEmptyString authorId, 
                uint additions, 
                uint deletions,
                Instant commitedAt, 
                NonEmptyString comment)
            {
                Id = id;
                RepositoryId = repositoryId;
                AuthorName = authorName;
                AuthorId = authorId;
                Additions = additions;
                Deletions = deletions;
                CommitedAt = commitedAt;
                Comment = comment;
            }
        }
    }
}

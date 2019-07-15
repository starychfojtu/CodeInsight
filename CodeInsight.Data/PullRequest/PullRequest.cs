using System;
using CodeInsight.Domain;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime.Extensions;

namespace CodeInsight.Data.PullRequest
{
    public sealed class PullRequest
    {
        public PullRequest(
            string id,
            string repositoryId,
            string title,
            string authorId,
            int deletions,
            int additions,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt,
            DateTimeOffset? mergedAt,
            DateTimeOffset? closedAt,
            int commentCount)
        {
            Id = id;
            RepositoryId = repositoryId;
            Title = title;
            AuthorId = authorId;
            Deletions = deletions;
            Additions = additions;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            MergedAt = mergedAt;
            ClosedAt = closedAt;
            CommentCount = commentCount;
        }

        public string Id { get; private set; }
        
        public string RepositoryId { get; private set; }

        public string Title { get; private set; }
        
        public string AuthorId { get; private set; }

        public int Deletions { get; private set; }
        
        public int Additions { get; private set; }
        
        public DateTimeOffset CreatedAt { get; private set; }
        
        public DateTimeOffset UpdatedAt { get; private set; }

        public DateTimeOffset? MergedAt { get; private set; }
        
        public DateTimeOffset? ClosedAt { get; private set; }
        
        public int CommentCount { get; private set; }

        public static PullRequest FromDomain(Domain.PullRequest.PullRequest pullRequest)
        {
            return new PullRequest(
                pullRequest.Id,    
                pullRequest.RepositoryId,
                pullRequest.Title,
                pullRequest.AuthorId,
                (int)pullRequest.Additions,
                (int)pullRequest.Deletions,
                pullRequest.CreatedAt.ToDateTimeOffset(),
                pullRequest.UpdatedAt.ToDateTimeOffset(),
                pullRequest.MergedAt.Map(m => m.ToDateTimeOffset()).ToNullable(),
                pullRequest.ClosedAt.Map(c => c.ToDateTimeOffset()).ToNullable(),
                (int)pullRequest.CommentCount
            );
        }
        
        public static Domain.PullRequest.PullRequest ToDomain(PullRequest pullRequest)
        {
            return new Domain.PullRequest.PullRequest(
                NonEmptyString.Create(pullRequest.Id).Get(),
                NonEmptyString.Create(pullRequest.RepositoryId).Get(),
                NonEmptyString.Create(pullRequest.Title).Get(),
                new AccountId(pullRequest.AuthorId),
                (uint)pullRequest.Additions,
                (uint)pullRequest.Deletions,
                pullRequest.CreatedAt.ToInstant(),
                pullRequest.UpdatedAt.ToInstant(),
                pullRequest.MergedAt.ToOption().Map(m => m.ToInstant()),
                pullRequest.ClosedAt.ToOption().Map(c => c.ToInstant()),
                (uint)pullRequest.CommentCount
            );
        }
    }
}
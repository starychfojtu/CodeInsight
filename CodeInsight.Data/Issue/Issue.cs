using System;
using System.Runtime.InteropServices;
using CodeInsight.Domain;
using CodeInsight.Library.Types;
using FuncSharp;
using NodaTime.Extensions;

namespace CodeInsight.Data.Issue
{
    public sealed class Issue
    {
        public Issue(
            int id, 
            string title,
            string url,
            string repositoryId, 
            DateTimeOffset? closedAt,
            DateTimeOffset lastUpdateAt, 
            int commentCount)
        {
            Id = id;
            Title = title;
            Url = url;
            RepositoryId = repositoryId;
            ClosedAt = closedAt;
            LastUpdateAt = lastUpdateAt;
            CommentCount = commentCount;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Url { get; private set; }

        public string RepositoryId { get; private set; }

        public DateTimeOffset? ClosedAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset LastUpdateAt { get; private set; }

        public int CommentCount { get; private set; }


        public static Issue FromDomain(Domain.Issue.Issue issue)
        {
            return new Issue(
                (int) issue.Id,
                issue.Title.Value,
                issue.Url.Value,
                issue.RepositoryId.Value,
                issue.ClosedAt.Map(c => c.ToDateTimeOffset()).ToNullable(),
                issue.LastUpdateAt.ToDateTimeOffset(),
                (int) issue.CommentCount
                );
        }

        public static Domain.Issue.Issue ToDomain(Issue issue)
        {
            return new Domain.Issue.Issue(
                (uint) issue.Id,
                NonEmptyString.Create(issue.Title).Get(),
                NonEmptyString.Create(issue.Url).Get(),
                NonEmptyString.Create(issue.RepositoryId).Get(),
                issue.ClosedAt.ToOption().Map(c => c.ToInstant()),
                issue.CreatedAt.ToInstant(),
                issue.LastUpdateAt.ToInstant(),
                (uint) issue.CommentCount
                );
        }
    }
}

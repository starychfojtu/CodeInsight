using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using NodaTime;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using static Octokit.GraphQL.Variable;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Import
{
    internal static class GetAllCommitsQuery
    {
        private static ICompiledQuery<IGitObject> Query { get; }

        static GetAllCommitsQuery()
        {
            Query = CreateQuery();
        }

        internal static IO<Task<IGitObject>> Execute(IConnection conn, Repository repository, int take, string cursor = null) => () =>
        {
            var vars = new Dictionary<string, object>
            {
                {"repositoryName", repository.Name.Value},
                {"repositoryOwner", repository.Owner.Value},
                {"after", cursor},
                {"first", take}
            };

            return conn.Run(Query, vars);
        };

        private static ICompiledQuery<IGitObject> CreateQuery() =>
            new Query()
                .Repository(Var("repositoryName"), Var("repositoryOwner"))
                .Ref("master")
                .Target.Compile();

        internal sealed class CommitDto
        {
            public NonEmptyString Id { get; private set; }

            public NonEmptyString RepositoryId { get; private set; }

            public NonEmptyString AuthorName { get; private set; }

            public NonEmptyString AuthorId { get; private set; }

            public uint Additions { get; private set; }

            public uint Deletions { get; private set; }

            public Instant CommittedAt { get; private set; }

            public NonEmptyString CommitMsg { get; private set; }

            public CommitDto(
                NonEmptyString id,
                NonEmptyString repositoryId,
                NonEmptyString authorName,
                NonEmptyString authorId,
                uint additions,
                uint deletions,
                Instant committedAt,
                NonEmptyString commitMsg)
            {
                Id = id;
                RepositoryId = repositoryId;
                AuthorName = authorName;
                AuthorId = authorId;
                Additions = additions;
                Deletions = deletions;
                CommittedAt = committedAt;
                CommitMsg = commitMsg;
            }
        }
    }
}

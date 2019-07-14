using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Library.Types;
using Monad;
using NodaTime;
using Octokit.GraphQL;
using Repository = CodeInsight.Domain.Repository.Repository;

namespace CodeInsight.Github.Queries
{
    internal static class GetAllCommitsQuery
    {
        internal static IO<Task<string>> Execute(IConnection conn, Repository repository, int take,
            string cursor = null) => () => conn.Run(CreateQuery(repository.Name.Value, repository.Owner.Value));

        //TODO: Parametrize correctly

        private static string CreateQuery(string repoName, string repoOwner)
        {
            var result = $@"query {{
                repository(name: ""{repoName}"", owner: ""{repoOwner}"") {{
                    ref (qualifiedName: ""master"") {{
                        target {{
                            ... on Commit {{
                                id
                                history(first: 2) {{
                                    pageInfo {{
                                        hasNextPage
                                    }}
                                    edges {{
                                        node {{
                                            messageHeadline
                                            oid
                                            message
                                            author {{
                                                name
                                                email
                                                date
                                            }}
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}";
            return result;
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

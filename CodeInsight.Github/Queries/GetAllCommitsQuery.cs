using System;
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
                                history(first: 2) {{
                                    pageInfo {{
                                        hasNextPage
                                    }}
                                    edges {{
                                        node {{
                                            id
                                            repositoryId: repository {{
                                                id
                                            }}
                                            authorName: author {{
                                                name
                                            }}
                                            authorId: author {{
                                                id
                                            }}
                                            additions
                                            deletions
                                            committedDate
                                            message
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
            public string Id { get; private set; }

            public string RepositoryId { get; private set; }

            public string AuthorName { get; private set; }

            public string AuthorId { get; private set; }

            public uint Additions { get; private set; }

            public uint Deletions { get; private set; }

            public DateTimeOffset CommittedAt { get; private set; }

            public string CommitMsg { get; private set; }
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Monad;
using Octokit.GraphQL;

namespace CodeInsight.Github.Queries
{
    public static class GetAllRepositoriesPrivateQuery
    {
        private static ICompiledQuery<IEnumerable<RepositoryDto>> Query { get; }

        static GetAllRepositoriesPrivateQuery()
        {
            Query = CreateQuery();
        }

        public static IO<Task<IEnumerable<RepositoryDto>>> Execute(IConnection connection) => () =>
            connection
                .Run(Query);

        private static ICompiledQuery<IEnumerable<RepositoryDto>> CreateQuery() =>
            new Query()
                .Viewer
                .Repositories(null, null, null, null, null, null, null, null, null, null)
                .AllPages()
                .Select(r => new RepositoryDto(r.Id.Value, r.Name, r.Owner.Login))
                .Compile();
    }
}

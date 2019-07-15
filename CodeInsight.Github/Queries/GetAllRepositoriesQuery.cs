using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Library.Extensions;
using Monad;
using Octokit.GraphQL;

namespace CodeInsight.Github.Queries
{
    public static class GetAllRepositoriesQuery
    {
        private static ICompiledQuery<IEnumerable<List<RepositoryDto>>> Query { get; }
        
        static GetAllRepositoriesQuery()
        {
            Query = CreateQuery();
        }

        public static IO<Task<IEnumerable<RepositoryDto>>> Execute(IConnection connection) => () =>
            connection
                .Run(Query)
                .Map(rs => rs.SelectMany(r => r));
        
        private static ICompiledQuery<IEnumerable<List<RepositoryDto>>> CreateQuery() =>
            new Query()
                .Viewer
                .Organizations()
                .AllPages()
                .Select(n => n
                    .Repositories(null, null, null, null, null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new RepositoryDto(r.Id.Value, r.Name, r.Owner.Login))
                    .ToList()
                )
                .Compile();
    }
}
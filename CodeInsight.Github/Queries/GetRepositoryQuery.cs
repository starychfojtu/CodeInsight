using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using FuncSharp;
using Monad;
using Octokit.GraphQL;
using static CodeInsight.Library.Prelude;
using IConnection = Octokit.GraphQL.IConnection;

namespace CodeInsight.Github.Queries
{
    public static class GetRepositoryQuery
    {
        private static ICompiledQuery<RepositoryDto> Query { get; }
        
        static GetRepositoryQuery()
        {
            Query = CreateQuery();
        }

        public static IO<Task<IOption<RepositoryDto>>> Execute(IConnection connection, NonEmptyString owner, NonEmptyString name)
        {
            return () =>
            {
                var vars = new Dictionary<string, object>
                {
                    {"repositoryName", name.Value},
                    {"repositoryOwner", owner.Value}
                };
                
                return connection.Run(Query, vars).SafeMap(r => r.MatchSingle(
                    repository => Some(repository),
                    e => throw e // TODO - Handle NotFoundException.
                ));
            };
        }
        
        private static ICompiledQuery<RepositoryDto> CreateQuery() =>
            new Query()
                .Repository(Variable.Var("repositoryName"), Variable.Var("repositoryOwner"))
                .Select(r => new RepositoryDto(r.Id.Value, r.Name, r.Owner.Login))
                .Compile();
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeInsight.Library.Types;
using Monad;
using Octokit.GraphQL;

namespace CodeInsight.Github.Queries
{
    internal static class GetRepositoryQuery
    {
        private static ICompiledQuery<RepositoryDto> Query { get; }
        
        static GetRepositoryQuery()
        {
            Query = CreateQuery();
        }

        internal static Reader<IConnection, Task<RepositoryDto>> Get(NonEmptyString owner, NonEmptyString name)
        {
            return conn =>
            {
                var vars = new Dictionary<string, object>
                {
                    {"repositoryName", name.Value},
                    {"repositoryOwner", owner.Value}
                };

                return conn.Run(Query, vars);
            };
        }
        
        private static ICompiledQuery<RepositoryDto> CreateQuery() =>
            new Query()
                .Repository(Variable.Var("repositoryName"), Variable.Var("repositoryOwner"))
                .Select(repository => new RepositoryDto
                {
                    Id = repository.Id.Value,
                    Name = repository.Name,
                    Owner = repository.Owner.Login
                })
                .Compile();
        
        
        internal sealed class RepositoryDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Owner { get; set; }
        }
    }
}
using CodeInsight.Domain.Repository;
using CodeInsight.Github;
using CodeInsight.Library.Types;
using FuncSharp;

namespace CodeInsight.Web.Common.Security
{
    public sealed class Client : Coproduct2<GithubRepositoryClient, Unit>
    {
        private Client(GithubRepositoryClient firstValue) : base(firstValue) {}
        private Client(Unit secondValue) : base(secondValue) {}

        public static Client Github(GithubRepositoryClient client) => new Client(client);
        public static Client None() => new Client(Unit.Value);

        public RepositoryId CurrentRepositoryId =>
            Match(
                github => github.RepositoryId,
                none => new RepositoryId(NonEmptyString.Create("Sample repo id").Get())
            );
    }
}
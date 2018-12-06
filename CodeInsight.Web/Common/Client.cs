using CodeInsight.Github;
using FuncSharp;

namespace CodeInsight.Web.Common
{
    public sealed class Client : Coproduct2<GithubRepositoryClient, Unit>
    {
        private Client(GithubRepositoryClient firstValue) : base(firstValue) {}
        private Client(Unit secondValue) : base(secondValue) {}

        public static Client Github(GithubRepositoryClient client) => new Client(client);
        public static Client None() => new Client(Unit.Value);
    }
}
using System.Threading.Tasks;
using CodeInsight.Library;
using FuncSharp;

namespace CodeInsight.Web.Common.Security
{
    public sealed class FakeClientAuthenticator : IClientAuthenticator
    {
        public Task<ITry<Client, AuthenticationError>> Authenticate(SignInParameters request)
        {
            return Try.Success<Client, AuthenticationError>(Client.None()).Async();
        }
    }
}
using System.Threading.Tasks;
using CodeInsight.Library;
using FuncSharp;

namespace CodeInsight.Web.Common.Security
{
    public sealed class FakeClientAuthenticator : IClientAuthenticator
    {
        public Task<ITry<Client>> Authenticate(SignInParameters request)
        {
            return Try.Success(Client.None()).Async();
        }
    }
}
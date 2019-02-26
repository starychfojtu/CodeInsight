using System.Threading.Tasks;
using FuncSharp;

namespace CodeInsight.Web.Common.Security
{
    public interface IClientAuthenticator
    {
        Task<ITry<Client, AuthenticationError>> Authenticate(SignInParameters request);
    }

    public enum AuthenticationError
    {
        RepositoryNotFound
    }
}
using System.Threading.Tasks;
using FuncSharp;

namespace CodeInsight.Web.Common.Security
{
    public interface IClientAuthenticator
    {
        Task<ITry<Client>> Authenticate(SignInParameters request);
    }
}
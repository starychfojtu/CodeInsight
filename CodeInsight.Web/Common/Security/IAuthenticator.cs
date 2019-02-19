using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeInsight.Web.Common.Security
{
    public interface IAuthenticator
    {
        Client Authenticate(HttpRequest request);
    }
}
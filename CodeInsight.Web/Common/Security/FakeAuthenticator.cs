using Microsoft.AspNetCore.Http;

namespace CodeInsight.Web.Common.Security
{
    public class FakeAuthenticator : IAuthenticator
    {
        public Client Authenticate(HttpRequest request)
        {
            return Client.None();
        }
    }
}
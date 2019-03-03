using System;
using System.Threading.Tasks;
using CodeInsight.Library;
using CodeInsight.Web.Common.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeInsight.Web.Common
{
    public abstract class AuthorizedController : Controller
    {
        private readonly ClientAuthenticator clientAuthenticator;

        protected AuthorizedController(ClientAuthenticator clientAuthenticator)
        {
            this.clientAuthenticator = clientAuthenticator;
        }
        
        protected Task<IActionResult> Action(Func<Client, Task<IActionResult>> f)
        {
            return clientAuthenticator.Authenticate(HttpContext).Match(
                client=> f(client),
                _ => Task.FromResult((IActionResult) new NotFoundResult())
            );
        }
    }
}
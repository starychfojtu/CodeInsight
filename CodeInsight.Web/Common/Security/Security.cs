using System;
using System.Threading.Tasks;
using CodeInsight.Library;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeInsight.Web.Common.Security
{
    public static class Security
    {
        public static Task<IActionResult> AuthorizedAction(IClientAuthenticator authenticator, HttpRequest request, Func<Client, Task<IActionResult>> f)
        {
            var notFound = Task.FromResult((IActionResult) new NotFoundResult());
            return GetSignInParameters(request.Cookies).Match(
                p => authenticator.Authenticate(p).Bind(client => client.Match(
                    c => f(c),
                    e => notFound
                )),
                _ => notFound
            );
        }
        
        private static IOption<SignInParameters> GetSignInParameters(IRequestCookieCollection cookies) =>
            from cookieName in cookies.Get("REPO_NAME")
            from name in NonEmptyString.Create(cookieName)
            from cookieOwner in cookies.Get("REPO_OWNER")
            from owner in NonEmptyString.Create(cookieOwner)
            select new SignInParameters(name, owner);
    }
}
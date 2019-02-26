using System;
using System.Threading.Tasks;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodeInsight.Web.Common.Security
{
    public static class Security
    {
        public static Task<IActionResult> AuthorizedAction(HttpContext httpContext, Func<Client, Task<IActionResult>> f)
        {
            return httpContext.Session.GetClient().Match(
                c => f(c),
                _ => Task.FromResult((IActionResult) new NotFoundResult())
            );
        }
    }
}
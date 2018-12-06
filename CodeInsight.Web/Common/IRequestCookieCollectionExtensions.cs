using CodeInsight.Library;
using FuncSharp;
using Microsoft.AspNetCore.Http;

namespace CodeInsight.Web.Common
{
    public static class IRequestCookieCollectionExtensions
    {
        public static IOption<string> Get(this IRequestCookieCollection collection, string key) =>
            collection.TryGetValue(key, out var value) ? Prelude.Some(value) : Prelude.None<string>();
    }
}
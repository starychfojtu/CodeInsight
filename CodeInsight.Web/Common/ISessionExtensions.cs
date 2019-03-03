using FuncSharp;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CodeInsight.Web.Common
{
    public static class ISessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static IOption<T> Get<T>(this ISession session, string key) =>
            session.GetString(key).ToOption().Map(JsonConvert.DeserializeObject<T>);
    }
}
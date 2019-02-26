using CodeInsight.Github;
using CodeInsight.Web.Common.Security;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CodeInsight.Web.Common
{
    public static class ISessionExtensions
    {
        private static readonly string ClientKey = "CLIENT";
        private static readonly string ClientTypeKey = "CLIENT_TYPE";
        
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static IOption<T> Get<T>(this ISession session, string key) =>
            session.GetString(key).ToOption().Map(JsonConvert.DeserializeObject<T>);

        public static void SetClient(this ISession session, Client client)
        {
            session.Set(ClientKey, client.CoproductValue);
            session.Set(ClientTypeKey, client.Match(
                github => ClientType.Github,
                none => ClientType.None
            ));
        }

        public static IOption<Client> GetClient(this ISession session)
        {
            var type = session.Get<ClientType>(ClientTypeKey);
            return type.FlatMap(t => t.Match(
                ClientType.None, _ => session.Get<Unit>(ClientKey).Map(n => Client.None()),
                ClientType.Github, _ => session.Get<GithubRepositoryClient>(ClientKey).Map(c => Client.Github(c))
            ));
        }

        private enum ClientType
        {
            None,
            Github
        }
    }
}
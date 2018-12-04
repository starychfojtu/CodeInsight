using System;
using CodeInsight.Github;
using CodeInsight.Library;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using static CodeInsight.Library.Prelude;
using Repository = CodeInsight.Domain.Repository;

namespace CodeInsight.Web.Common
{
    public static class Authorization
    {
        public static IActionResult AuthorizedAction(HttpRequest request, Func<GithubRepositoryClient, IActionResult> action)
        {
            var githubClient = new GitHubClient(new ProductHeaderValue("starychfojtu"));
            return GetRepository(request.Cookies)
                .Map(repo => new GithubRepositoryClient(githubClient, repo))
                .Map(action)
                .GetOrElse(new NotFoundResult());
        }
        
        private static IOption<Repository> GetRepository(IRequestCookieCollection cookies) =>
            from cookieName in cookies.Get("REPO_NAME")
            from name in NonEmptyString.Create(cookieName)
            from cookieOwner in cookies.Get("REPO_OWNER")
            from owner in NonEmptyString.Create(cookieOwner)
            select new Repository(name, owner);
    }

    // TODO: Move extensions
    public static class IRequestCookieCollectionExtensions
    {
        public static IOption<string> Get(this IRequestCookieCollection collection, string key) =>
            collection.TryGetValue(key, out var value) ? Some(value) : None<string>();
    }
    
    public static class IOptionExtensions
    {
        public static IOption<B> Select<A, B>(this IOption<A> option, Func<A, B> f) =>
            option.Map(f);
        
        public static IOption<C> SelectMany<A, B, C>(this IOption<A> option, Func<A, IOption<B>> bind, Func<A, B, C> project) =>
            option.FlatMap(a => bind(a).Map(b => project(a, b)));
    }
}
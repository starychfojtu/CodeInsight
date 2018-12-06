using System;
using FuncSharp;

namespace CodeInsight.Web.Common
{
    public static class IOptionExtensions
    {
        public static IOption<B> Select<A, B>(this IOption<A> option, Func<A, B> f) =>
            option.Map(f);
        
        public static IOption<C> SelectMany<A, B, C>(this IOption<A> option, Func<A, IOption<B>> bind, Func<A, B, C> project) =>
            option.FlatMap(a => bind(a).Map(b => project(a, b)));
    }
}
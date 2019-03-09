using System;
using FuncSharp;

namespace CodeInsight.Library.Extensions
{
    public static class IOptionExtensions
    {
        public static ITry<A, E> ToTry<A, E>(this IOption<A> option, Func<Unit, E> getError) =>
            option.Match(
                v => Try.Success<A, E>(v),
                _ => Try.Error<A, E>(getError(Unit.Value))
            );
    }
}
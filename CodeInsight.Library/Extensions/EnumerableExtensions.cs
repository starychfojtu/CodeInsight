using System;
using System.Collections.Generic;
using FuncSharp;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Library.Extensions
{
    public static class EnumerableExtensions
    {
        public static (IEnumerable<A> first, IEnumerable<B> second) BiSelect<A, B, T>(this IEnumerable<T> items, Func<T, A> firstMap, Func<T, B> secondMap)
        {
            var first = new List<A>();
            var second = new List<B>();

            foreach (var item in items)
            {
                first.Add(firstMap(item));
                second.Add(secondMap(item));
            }

            return (first, second);
        }
        
        public static (IReadOnlyList<T> passing, IReadOnlyList<T> violating) Partition<T>(this IEnumerable<T> items, Predicate<T> predicate)
        {
            var passing = new List<T>();
            var violating = new List<T>();

            foreach (var item in items)
            {
                predicate(item).Match(
                    t => passing.Add(item),
                    f => violating.Add(item)
                );
            }

            return (passing, violating);
        }

        public static IOption<A> ElementAt<A>(this A[] array, int index) =>
            array.Length > index ? Some(array[index]) : None<A>();
    }
}
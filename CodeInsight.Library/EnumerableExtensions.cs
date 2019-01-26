using System;
using System.Collections.Generic;

namespace CodeInsight.Library
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
    }
}
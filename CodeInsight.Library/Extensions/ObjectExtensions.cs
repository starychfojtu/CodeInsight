using System;
using FuncSharp;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Library.Extensions
{
    public static class ObjectExtensions
    {
        public static A Max<A>(this A a, A b)
            where A : IComparable<A>
        {
            return a.CompareTo(b) == 1 ? a : b;
        }
        
        public static A Min<A>(this A a, A b)
            where A : IComparable<A>
        {
            return a.CompareTo(b) == 1 ? b : a;
        }

        public static IOption<A> AsOption<A>(this A a) =>
            a.ToOption();

        public static IOption<A> Cast<A>(this object obj)
        {
            try
            {
                return Some((A) obj);
            }
            catch (InvalidOperationException)
            {
                return None<A>();
            }
        }
    }
}
using System;

namespace CodeInsight.Library
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
    }
}
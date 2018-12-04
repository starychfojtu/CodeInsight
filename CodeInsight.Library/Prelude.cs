using FuncSharp;

namespace CodeInsight.Library
{
    public static class Prelude
    {
        public static IOption<A> Some<A>(A value) =>
            Option.Create(value);
        
        public static IOption<A> None<A>() =>
            Option.Empty<A>();
    }
}
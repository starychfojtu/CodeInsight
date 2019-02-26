using FuncSharp;

namespace CodeInsight.Library
{
    public static class Prelude
    {
        public static IOption<A> Some<A>(A value) =>
            Option.Create(value);
        
        public static IOption<A> None<A>() =>
            Option.Empty<A>();

        public static IOption<int> SafeDiv(int dividend, int divisor) => 
            divisor == 0 ? None<int>() : Some(dividend / divisor);
    }
}
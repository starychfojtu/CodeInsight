using FuncSharp;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Library
{
    public sealed class NonEmptyString : NewType<string>
    {
        private NonEmptyString(string value) : base(value) {}
        
        public static IOption<NonEmptyString> Create(string value) =>
            string.IsNullOrEmpty(value) ? None<NonEmptyString>() : Some(new NonEmptyString(value));
    }
}
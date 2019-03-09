using FuncSharp;

namespace CodeInsight.Library.Types
{
    public sealed class NonEmptyString : NewType<string>
    {
        private NonEmptyString(string value) : base(value) {}
        
        public static IOption<NonEmptyString> Create(string value) =>
            string.IsNullOrEmpty(value) ? Prelude.None<NonEmptyString>() : Prelude.Some(new NonEmptyString(value));
    }
}
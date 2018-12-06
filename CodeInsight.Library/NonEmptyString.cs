using FuncSharp;
using static CodeInsight.Library.Prelude;

namespace CodeInsight.Library
{
    public struct NonEmptyString
    {
        private NonEmptyString(string value) => 
            Value = value;

        public string Value { get; }

        public static implicit operator string(NonEmptyString s) =>
            s.Value;

        public static IOption<NonEmptyString> Create(string value) =>
            string.IsNullOrEmpty(value) ? None<NonEmptyString>() : Some(new NonEmptyString(value));

        public override string ToString() =>
            Value;
    }
}
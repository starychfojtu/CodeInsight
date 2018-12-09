using CodeInsight.Library;

namespace CodeInsight.Domain
{
    public sealed class AccountId : NewType<string>
    {
        public AccountId(string value) : base(value)
        {
        }
    }
}
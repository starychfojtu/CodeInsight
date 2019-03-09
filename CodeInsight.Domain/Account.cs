using CodeInsight.Library;

namespace CodeInsight.Domain
{
    public sealed class AccountId : NewType<string>
    {
        public AccountId(string value) : base(value) {}
    }
    
    public class Account
    {
        public Account(AccountId id)
        {
            Id = id;
        }

        public AccountId Id { get; }
    }
}
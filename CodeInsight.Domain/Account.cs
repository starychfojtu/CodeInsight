namespace CodeInsight.Domain
{
    public class Account
    {
        public Account(AccountId id)
        {
            Id = id;
        }

        public AccountId Id { get; }
    }
}
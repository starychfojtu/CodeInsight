using CodeInsight.Library;

namespace CodeInsight.Web.Common.Security
{
    public class SignInParameters
    {
        public SignInParameters(NonEmptyString owner, NonEmptyString repository)
        {
            Owner = owner;
            Repository = repository;
        }

        public NonEmptyString Owner { get; }
        
        public NonEmptyString Repository { get; }
    }
}
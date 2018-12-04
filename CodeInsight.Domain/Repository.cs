using CodeInsight.Library;

namespace CodeInsight.Domain
{
    public class Repository
    {
        public Repository(NonEmptyString name, NonEmptyString ownerName)
        {
            Name = name;
            OwnerName = ownerName;
        }

        public NonEmptyString Name { get; }
        
        public NonEmptyString OwnerName { get; }
    }
}
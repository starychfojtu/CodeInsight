using CodeInsight.Library;

namespace CodeInsight.Domain
{
    public class Repository
    {
        public Repository(NonEmptyString id, NonEmptyString name, NonEmptyString owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }

        public NonEmptyString Id { get; }
        
        public NonEmptyString Name { get; }
        
        public NonEmptyString Owner { get; }
    }
}
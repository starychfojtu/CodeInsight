using CodeInsight.Library;

namespace CodeInsight.Domain
{
    public sealed class RepositoryId : NewType<NonEmptyString>
    {
        public RepositoryId(NonEmptyString value) : base(value) {}
    }
    
    public class Repository
    {
        public Repository(RepositoryId id, NonEmptyString name, NonEmptyString owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }

        public RepositoryId Id { get; }
        
        public NonEmptyString Name { get; }
        
        public NonEmptyString Owner { get; }
    }
}
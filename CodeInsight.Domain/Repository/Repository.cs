using CodeInsight.Library;
using CodeInsight.Library.Types;

namespace CodeInsight.Domain.Repository
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
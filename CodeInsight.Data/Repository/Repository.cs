using CodeInsight.Domain;
using CodeInsight.Library;

namespace CodeInsight.Data.Repository
{
    public sealed class Repository
    {
        public Repository(string id, string name, string owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }

        public string Id { get; private set; }
        
        public string Name { get; private set; }
        
        public string Owner { get; private set; }
        
        public static Repository FromDomain(Domain.Repository repository)
        {
            return new Repository(
                repository.Id.Value,
                repository.Name,
                repository.Owner
            );
        }
        
        public static Domain.Repository ToDomain(Repository repository)
        {
            return new Domain.Repository(
                new RepositoryId(NonEmptyString.Create(repository.Id).Get()),
                NonEmptyString.Create(repository.Name).Get(),
                NonEmptyString.Create(repository.Owner).Get()
            );
        }
    }
}
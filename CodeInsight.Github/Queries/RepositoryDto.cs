namespace CodeInsight.Github.Queries
{
    public sealed class RepositoryDto
    {
        public RepositoryDto(string id, string name, string owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
        }

        public string Id { get; }
        public string Name { get; }
        public string Owner { get; }
    }
}
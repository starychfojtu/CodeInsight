using System.Collections.Generic;

namespace CodeInsight.Web.Models.Github
{
    public sealed class RepositoryItem
    {
        public RepositoryItem(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; }
        public string Name { get; }
    }
    
    public sealed class ChooseRepositoryViewModel
    {
        public ChooseRepositoryViewModel(IEnumerable<RepositoryItem> repositories)
        {
            Repositories = repositories;
        }

        public IEnumerable<RepositoryItem> Repositories { get; }
    }
}
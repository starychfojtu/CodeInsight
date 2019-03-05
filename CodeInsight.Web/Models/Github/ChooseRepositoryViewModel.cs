using System.Collections.Generic;

namespace CodeInsight.Web.Models.Github
{
    public sealed class RepositoryInputDto
    {
        public RepositoryInputDto(string name, string owner)
        {
            Name = name;
            Owner = owner;
        }

        public string Name { get; }
        
        public string Owner { get; }
    }
    
    public sealed class ChooseRepositoryViewModel
    {
        public ChooseRepositoryViewModel(IEnumerable<RepositoryInputDto> repositories)
        {
            Repositories = repositories;
        }

        public IEnumerable<RepositoryInputDto> Repositories { get; }
    }
}
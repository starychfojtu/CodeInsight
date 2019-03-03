using System.Collections.Generic;

namespace CodeInsight.Web.Models.Github
{
    public sealed class RepositoryInputDto
    {
        public RepositoryInputDto(string name)
        {
            Name = name;
        }

        public string Name { get; }
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
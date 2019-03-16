using System.Collections.Generic;
using FuncSharp;

namespace CodeInsight.Web.Models.Github
{
    public sealed class RepositoryInputDto
    {
        public RepositoryInputDto(string name, string owner)
        {
            NameWithOwner = $"{owner}/{name}";
        }

        public string NameWithOwner { get; }
    }
    
    public sealed class ChooseRepositoryViewModel
    {
        public ChooseRepositoryViewModel(IEnumerable<RepositoryInputDto> repositories, IOption<string> error)
        {
            Repositories = repositories;
            Error = error;
        }

        public IEnumerable<RepositoryInputDto> Repositories { get; }
        
        public IOption<string> Error { get; }
    }
}
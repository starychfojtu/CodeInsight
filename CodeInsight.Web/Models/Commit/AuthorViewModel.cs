using System.Collections.Generic;
using CodeInsight.Commits;

namespace CodeInsight.Web.Models.Commit
{
    public class AuthorViewModel
    {
        public AuthorViewModel(IReadOnlyList<AuthorStats> authors) 
        {
            Authors = authors;
        }

        public IReadOnlyList<AuthorStats> Authors { get; }
    }
}

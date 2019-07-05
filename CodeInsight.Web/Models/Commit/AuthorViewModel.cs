using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Commits;
using CodeInsight.Web.Common.Charts;

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

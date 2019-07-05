using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Commits;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.Commit
{
    public class AuthorViewModel : ChartsViewModel
    {
        public AuthorViewModel(
            IReadOnlyList<LifetimeAuthorStats> authors,
            IReadOnlyList<Chart> charts) 
            : base(charts)
        {
            Authors = authors;
        }

        public IReadOnlyList<LifetimeAuthorStats> Authors { get; }
    }
}

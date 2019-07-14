using System.Collections.Generic;
using CodeInsight.Web.Common.Charts;

namespace CodeInsight.Web.Models.Commit
{
    public class CodeTabModel : ChartsViewModel
    {
        public CodeTabModel(IReadOnlyList<Chart> charts) : base(charts)
        {
        }
    }
}
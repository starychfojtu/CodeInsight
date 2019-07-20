using System.Collections.Generic;
using CodeInsight.Commits;
using CodeInsight.Domain.Issue;

namespace CodeInsight.Web.Models.Commit
{
    public class IssueViewModel
    {
        public IssueViewModel(IReadOnlyList<Issue> issues)
        {
            Issues = issues;
        }

        public IReadOnlyList<Issue> Issues { get; }
    }
}

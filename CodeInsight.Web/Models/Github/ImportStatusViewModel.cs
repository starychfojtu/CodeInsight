using System;

namespace CodeInsight.Web.Models.Github
{
    public class ImportStatusViewModel
    {
        public ImportStatusViewModel(Guid jobId)
        {
            JobId = jobId;
        }
        
        public Guid JobId { get; }
    }
}
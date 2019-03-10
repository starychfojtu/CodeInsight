namespace CodeInsight.Web.Models.Github
{
    public class ImportStatusViewModel
    {
        public ImportStatusViewModel(uint progress)
        {
            Progress = progress;
        }

        public uint Progress { get; }
    }
}
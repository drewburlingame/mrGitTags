using CommandDotNet;

namespace mrGitTags
{
    public class CommitsAndFilesArgs : IArgumentModel
    {
        [Option(ShortName = "f", LongName = "show-files",
            Description = "list all files changed within each project")]
        public bool ShowFiles { get; set; }

        [Option(ShortName = "c", LongName = "show-commits")]
        public bool ShowCommits { get; set; }
    }
}
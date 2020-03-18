using System.IO;
using CommandDotNet;

namespace mrGitTags
{
    public class RepoAppOptions : IArgumentModel
    {
        [Option(LongName = "repo-dir")]
        public string RepoDirectory { get; set; } = Directory.GetCurrentDirectory();
    }
}
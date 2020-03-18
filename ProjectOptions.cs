using CommandDotNet;

namespace mrGitTags
{
    public class ProjectOptions : IArgumentModel
    {
        [Option(ShortName = "p", LongName = "project", Description = "The id or name of the project")]
        public string Project { get; set; }
    }
}
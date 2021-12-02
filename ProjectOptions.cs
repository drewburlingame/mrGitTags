using CommandDotNet;

namespace mrGitTags
{
    public class ProjectOptions : IArgumentModel
    {
        [Option('p', "project", Description = "The id or name of the project")]
        public string? Project { get; set; }
    }
}
using CommandDotNet;

namespace mrGitTags
{
    public class ProjectsOptions : IArgumentModel
    {
        [Option(ShortName = "p", Description = "The id or name of the project")]
        public string[] Projects { get; set; } = new string[0];
    }
}
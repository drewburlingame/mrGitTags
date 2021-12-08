using CommandDotNet;

namespace mrGitTags;

public class TagsArgs : IArgumentModel
{
    [Option("pre", Description = "include prerelease tags")]
    public bool IncludePrereleases { get; set; }
}
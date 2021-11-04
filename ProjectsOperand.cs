using System;
using CommandDotNet;

namespace mrGitTags
{
    public class ProjectsOperand : IArgumentModel
    {
        [Operand(Description = "The name or index of the project")]
        public string[] Projects { get; set; } = Array.Empty<string>();
    }
}
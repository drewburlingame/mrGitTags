﻿using System.IO;
using CommandDotNet;

namespace mrGitTags
{
    public class RepoAppOptions : IArgumentModel
    {
        [Option(LongName = "repo-dir")]
        public string RepoDirectory { get; set; } = Directory.GetCurrentDirectory();

        [Option(LongName = "branch", ShortName = "b",
            Description = "The branch to use as head. Use `-b current` or `-b !` to use the current branch.")]
        public string Branch { get; set; } = "master";
    }
}
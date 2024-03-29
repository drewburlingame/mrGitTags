﻿using CommandDotNet;

namespace mrGitTags
{
    public class CommitsAndFilesArgs : IArgumentModel
    {
        [Option('f', Description = "list all files changed within each project")]
        public bool ShowFiles { get; set; }

        [Option('c', Description = "list all commits within each project")]
        public bool ShowCommits { get; set; }
    }
}
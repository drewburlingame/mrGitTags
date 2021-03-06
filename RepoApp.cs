﻿using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Prompts;
using CommandDotNet.Rendering;
using LibGit2Sharp;
using MoreLinq.Extensions;
using Pastel;
using Semver;

namespace mrGitTags
{
    public class RepoApp
    {
        private Repo _repo;
        private IConsole _console;
        private IndentableStreamWriter _writer;
        private Action<string> _writeln;

        public Task<int> Intercept(
            InterceptorExecutionDelegate next,
            RepoAppOptions options,
            IConsole console)
        {
            _console = console;
            _writer = new IndentableStreamWriter(console.Out);
            _writeln = _writer.WriteLine;

            _repo = new Repo(options.RepoDir, options.Branch);
            return next();
        }

        [Command(Description = "list the projects in the repo")]
        public void projects()
        {
            _repo.GetProjects().ForEach(p => _writeln($"{p.Theme_ProjectIndexAndName()}"));
        }

        [Command(Description = "list all tags for the given project(s)")]
        public void tags(
            CancellationToken cancellationToken,
            ProjectsOptions projectsOptions,
            CommitsAndFilesArgs commitsAndFilesArgs,
            [Option(ShortName = "i")] 
            bool includePrereleases = false,
            [Option(ShortName = "d", LongName = "depth", Description = "How many tags to show per project")] 
            int tagCount = 3,
            [Option(LongName = "ltev", Description = "show all tags less than or equal to the version")]
            SemVersion skipToVersion = null,
            [Option(LongName = "gtv", Description = "show all tags greater than the version")] 
            SemVersion untilVersion = null)
        {
            foreach (var project in _repo.GetProjects(projectsOptions))
            {
                _writeln(null);
                _writeln($"{project.Theme_ProjectIndexAndName()}");
                using (_writer.Indent())
                {
                    _writeln($"{project.Branch.FriendlyName.Theme_GitNameAlt()}");

                    bool printedNextIfExists = false;
                    foreach (var tagInfo in _repo.GetTagsOrEmpty(project.Name)
                        .Where(t => !t.IsPrerelease || includePrereleases)
                        .SkipUntil(t => skipToVersion == null || t.SemVersion <= skipToVersion)
                        .Take(untilVersion == null ? tagCount : int.MaxValue)
                        .TakeWhile(t => untilVersion == null || t.SemVersion >= untilVersion)
                        .TakeUntil(_ => cancellationToken.IsCancellationRequested))
                    {
                        if (!printedNextIfExists)
                        {
                            if (tagInfo.Next != null)
                            {
                                var nextTaggedCommit = _repo.Git.Lookup<Commit>(tagInfo.Next.Tag.Target.Id);
                                _writeln($"{tagInfo.Next.FriendlyName.Theme_GitNameAlt()} {nextTaggedCommit.Committer.Theme_WhenDate()}  {nextTaggedCommit.Author.Theme_Name()}");
                            }
                            printedNextIfExists = true;
                        }
                        using (_writer.Indent())
                        {
                            WriteCommitsAndFiles(_writer, project, tagInfo, commitsAndFilesArgs, cancellationToken);
                        }

                        var taggedCommit = _repo.Git.Lookup<Commit>(tagInfo.Tag.Target.Id);
                        _writeln($"{tagInfo.FriendlyName.Theme_GitNameAlt()} {taggedCommit.Committer.Theme_WhenDate()}  {taggedCommit.Author.Theme_Name()}");
                    }
                }
            }
        }

        [Command(Description = "create and push a tag for the next version of the project")]
        public void increment(
            [Operand(Name = "project", Description = "The id or name of the project")] string projectKey,
            [Option(ShortName = "t")] SemVerElement type = SemVerElement.patch)
        {
            var project = _repo.GetProjectOrDefault(projectKey) ?? throw new ArgumentException($"unknown project:{projectKey}");
            var nextTag = project.Increment(type);
            _writeln($"added {nextTag.FriendlyName}");
            _writeln("run the following command to push the tag to the remote");
            _writeln(null);
            _writeln($"git push origin {nextTag.FriendlyName}".Theme_GitLinks());
        }

        [Command(Description = "Status each project for since the last tag of each project to the head of the current branch.")]
        public void status(
            IPrompter prompter,
            CancellationToken cancellationToken,
            ProjectsOptions projectsOptions,
            CommitsAndFilesArgs commitsAndFilesArgs,
            [Option(ShortName = "s", Description = "list only the project and change summary")] bool summaryOnly = false,
            [Option(ShortName = "i", Description = "prompt to increment version for each project with changes")] bool interactive = false)
        {
            foreach (var project in _repo.GetProjects(projectsOptions))
            {
                _writeln(null);
                if (project.LatestTag == null)
                {
                    _writeln($"{project.Theme_ProjectIndexAndName()}: {"no tag".Pastel(Color.Orange)}");
                    continue;
                }

                var taggedCommit = project.LatestTaggedCommit;
                var tip = project.Branch.Tip;

                var changes = project.GetFilesChangedSinceLatestTag(tip).ToList();

                _writeln($"{project.Theme_ProjectIndexAndName()}: {changes.Summary()}");
                if (!summaryOnly)
                {
                    using (_writer.Indent())
                    {
                        _writeln($"branch: {tip.Committer.Theme_WhenDateTime()} {project.Branch.FriendlyName.Theme_GitName()} {tip.Author.Theme_Name()}");
                        _writeln($"        {tip.MessageShort}");
                        _writeln($"tag   : {taggedCommit.Committer.Theme_WhenDateTime()} {project.LatestTag.Tag.FriendlyName.Theme_GitName()} {taggedCommit.Author.Theme_Name()}");
                        _writeln($"        {taggedCommit.MessageShort}");

                        if (changes.Any())
                        {
                            using (_writer.Indent())
                            {
                                WriteCommitsAndFiles(_writer, project, project.LatestTag, 
                                    commitsAndFilesArgs,
                                    cancellationToken);
                            }

                            if (interactive)
                            {
                                var response = prompter.PromptForValue("increment version? [major(j)>,minor(n),patch(p),skip(s)]", out bool isCancellationRequested);
                                if (isCancellationRequested)
                                {
                                    return;
                                }

                                if (response.Equals("major", StringComparison.OrdinalIgnoreCase) || response.Equals("m", StringComparison.OrdinalIgnoreCase))
                                {
                                    this.increment(project.Name, SemVerElement.major);
                                }
                                else if (response.Equals("minor", StringComparison.OrdinalIgnoreCase) || response.Equals("n", StringComparison.OrdinalIgnoreCase))
                                {
                                    this.increment(project.Name, SemVerElement.minor);
                                }
                                else if (response.Equals("patch", StringComparison.OrdinalIgnoreCase) || response.Equals("p", StringComparison.OrdinalIgnoreCase))
                                {
                                    this.increment(project.Name, SemVerElement.patch);
                                }
                            }

                        }
                    }
                }
            }
        }

        private void WriteCommitsAndFiles(IndentableStreamWriter writer, Project project, TagInfo tagInfo,
            CommitsAndFilesArgs args, CancellationToken cancellationToken)
        {
            var nextTagInfo = tagInfo.Next;
            var target = tagInfo.Tag.Target;
            var nextTarget = nextTagInfo?.Tag.Target ?? project.Tip;

            // https://git-scm.com/book/en/v2/Git-Tools-Revision-Selection  #Commit Ranges
            var commitRange = $"{tagInfo.FriendlyName}...{nextTagInfo?.FriendlyName ?? project.Branch.FriendlyName}";

            //https://github.com/bilal-fazlani/commanddotnet/compare/CommandDotNet_3.5.0...CommandDotNet_3.5.1
            writer.WriteLine($"{_repo.GetOriginRepoUrl().HttpsUrl}/compare/{commitRange}".Theme_GitLinks());

            if (args.ShowCommits)
            {
                writer.WriteLine($"git log --graph {commitRange} -- {project.Directory}".Theme_GitLinks());
                writer.WriteLine($"git log --oneline --graph {commitRange} -- {project.Directory}".Theme_GitLinks());

                foreach (var commit in project
                    .GetCommitsBetween(target, nextTarget, cancellationToken)
                    .TakeUntil(_ => cancellationToken.IsCancellationRequested))
                {
                    writer.WriteLine(
                        $"{commit.ShortSha()} {commit.Author.Theme_WhenDateTime()} {commit.MessageShort.Theme_GitMessage()}");
                }
            }

            if (args.ShowFiles)
            {
                // https://stackoverflow.com/questions/1552340/how-to-list-only-the-file-names-that-changed-between-two-commits/6827937

                writer.WriteLine($"git diff {commitRange} -- {project.Directory}".Theme_GitLinks());
                writer.WriteLine($"git diff --name-status {commitRange} -- {project.Directory}".Theme_GitLinks());

                foreach (var file in project.GetFilesChangedBetween(target, nextTarget))
                {
                    writer.WriteLine($"{file.Theme_FileChange()}");
                }
            }
        }
    }
}
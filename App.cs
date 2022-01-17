using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Prompts;
using LibGit2Sharp;
using MoreLinq.Extensions;
using Pastel;
using Semver;
using Spectre.Console;
using Color = System.Drawing.Color;

namespace mrGitTags
{
    public class App
    {
        private Repo _repo = null!;
        private IndentableStreamWriter _writer = null!;
        private Action<string?> _writeln = null!;
        private IAnsiConsole _console = null!;
        private CancellationToken _cancellationToken;
        private CliWrapper _cliWrapper = null!;

        public Task<int> Intercept(
            InterceptorExecutionDelegate next,
            RepoAppOptions options,
            CommandContext commandContext,
            IAnsiConsole console,
            CancellationToken cancellationToken)
        {
            _console = console;
            _cancellationToken = cancellationToken;
            _writer = new IndentableStreamWriter(console.WriteLine);
            _writeln = _writer.WriteLine;

            _repo = options.GetRepo();
            _cliWrapper = new CliWrapper(commandContext);
            return next();
        }

        [Command(Description = "list the projects in the repo")]
        public void projects()
        {
            _repo.GetProjects().ForEach(p => _writeln($"{p.Theme_ProjectIndexAndName()}"));
        }

        [Command(Description = "list all tags for the given project(s)")]
        public void tags(
            CommitsAndFilesArgs commitsAndFilesArgs,
            TagsArgs tagsArgs,
            ProjectsOperand projectsOperand,
            [Option('d', "depth", Description = "How many tags to show per project")] 
            int tagCount = 3,
            [Option("lte", Description = "show all tags less than or equal to the version")]
            SemVersion? skipToVersion = null,
            [Option("gt", Description = "show all tags greater than the version")] 
            SemVersion? untilVersion = null)
        {
            foreach (var project in _repo.GetProjects(projectsOperand))
            {
                _writeln(null);
                _writeln($"{project.Theme_ProjectIndexAndName()}");
                using (_writer.Indent())
                {
                    _writeln($"{project.Branch.FriendlyName.Theme_GitNameAlt()}");

                    bool printedNextIfExists = false;
                    foreach (var tagInfo in project.Tags
                        .Where(t => !t.IsPrerelease || tagsArgs.IncludePrereleases)
                        .SkipUntil(t => skipToVersion == null || t.SemVersion <= skipToVersion)
                        .Take(untilVersion == null ? tagCount : int.MaxValue)
                        .TakeWhile(t => untilVersion == null || t.SemVersion >= untilVersion)
                        .TakeUntil(_ => _cancellationToken.IsCancellationRequested))
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
                            WriteCommitsAndFiles(project, tagInfo, commitsAndFilesArgs, tagsArgs, _cancellationToken);
                        }

                        var taggedCommit = _repo.Git.Lookup<Commit>(tagInfo.Tag.Target.Id);
                        _writeln($"{tagInfo.FriendlyName.Theme_GitNameAlt()} {taggedCommit.Committer.Theme_WhenDate()}  {taggedCommit.Author.Theme_Name()}");
                    }
                }
            }
        }

        [Command(Description = "create and push a tag for the next version of the project")]
        public void increment(
            [Operand("project", Description = "The id or name of the project")] string project,
            [Option('t')] SemVerElement type = SemVerElement.patch,
            [Option('p')] bool pushTagsToRemote = false)
        {
            // TODO: use spectre to prompt for project.

            var p = _repo.GetProjectOrDefault(project) ?? throw new ArgumentException($"unknown project:{project}");
            var nextTag = p.Increment(type);
            _writeln($"added {nextTag.FriendlyName}");
            if (pushTagsToRemote)
            {
                _writeln($"pushing {nextTag.FriendlyName} to remote");
                _cliWrapper.Execute("git", $"push origin {nextTag.FriendlyName}");
            }
            else
            {
                _writeln("run the following command to push the tag to the remote");
                _writeln(null);
                _writeln($"git push origin {nextTag.FriendlyName}");
            }
        }

        [Command(Description = "Status each project for since the last tag of each project to the head of the current branch.")]
        public void status(
            IPrompter prompter,
            ProjectsOperand projectsOperand,
            CommitsAndFilesArgs commitsAndFilesArgs,
            TagsArgs tagsArgs,
            [Option('m', Description = "show only projects with changes")] bool modifiedOnly = false,
            [Option('s', Description = "list only the project and change summary")] bool summaryOnly = false,
            [Option('i', Description = "prompt to increment version for each project with changes")] bool interactive = false,
            [Option('p')] bool pushTagsToRemote = false)
        {
            // TODO: use spectre to write the table.
            foreach (var project in _repo.GetProjects(projectsOperand))
            {
                var tip = project.Branch.Tip;
                var changes = project.GetFilesChangedSinceLatestTag(tip).ToList();

                if (modifiedOnly && !changes.Any())
                {
                    continue;
                }

                var latestTag = project.LatestTag(tagsArgs.IncludePrereleases);

                _writeln(null);
                _writeln(latestTag is null
                    ? $"{project.Theme_ProjectIndexAndName()}: {"no tag".Pastel(Color.Orange)}"
                    : $"{project.Theme_ProjectIndexAndName()}: {changes.Summary()}");

                if (!summaryOnly)
                {
                    using (_writer.Indent())
                    {
                        _writeln($"branch: {tip.Committer.Theme_WhenDateTime()} {project.Branch.FriendlyName.Theme_GitName()} {tip.Author.Theme_Name()}");
                        _writeln($"        {tip.MessageShort}");

                        if (latestTag is not null)
                        {
                            var taggedCommit = latestTag.Commit!;
                            _writeln($"tag   : {taggedCommit.Committer.Theme_WhenDateTime()} {latestTag.Tag.FriendlyName.Theme_GitName()} {taggedCommit.Author.Theme_Name()}");
                            _writeln($"        {taggedCommit.MessageShort}");
                        }

                        if (changes.Any())
                        {
                            using (_writer.Indent())
                            {
                                WriteCommitsAndFiles(project, latestTag, commitsAndFilesArgs, tagsArgs, _cancellationToken);
                            }

                            if (interactive)
                            {
                                var response = prompter.PromptForValue(
                                    "increment version? [major(j)>,minor(n),patch(p),skip(s)]", 
                                    out bool isCancellationRequested);
                                if (isCancellationRequested || response is null)
                                {
                                    return;
                                }

                                if (response.Equals("major", StringComparison.OrdinalIgnoreCase) || response.Equals("j", StringComparison.OrdinalIgnoreCase))
                                {
                                    increment(project.Name, SemVerElement.major, pushTagsToRemote);
                                }
                                else if (response.Equals("minor", StringComparison.OrdinalIgnoreCase) || response.Equals("n", StringComparison.OrdinalIgnoreCase))
                                {
                                    increment(project.Name, SemVerElement.minor, pushTagsToRemote);
                                }
                                else if (response.Equals("patch", StringComparison.OrdinalIgnoreCase) || response.Equals("p", StringComparison.OrdinalIgnoreCase))
                                {
                                    increment(project.Name, SemVerElement.patch, pushTagsToRemote);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void WriteCommitsAndFiles(Project project, TagInfo? tagInfo,
            CommitsAndFilesArgs args, TagsArgs tagsArgs, CancellationToken cancellationToken)
        {
            var nextTagInfo = tagInfo?.Next;
            if (nextTagInfo is not null && nextTagInfo.IsPrerelease && !tagsArgs.IncludePrereleases)
            {
                nextTagInfo = null;
            }
            var target = tagInfo?.Tag.Target ?? project.Repo.FirstCommit;
            var nextTarget = nextTagInfo?.Tag.Target ?? project.Tip;

            // https://git-scm.com/book/en/v2/Git-Tools-Revision-Selection  #Commit Ranges
            var from = tagInfo?.FriendlyName ?? target.Sha;
            var to = nextTagInfo?.FriendlyName ?? project.Branch.FriendlyName;
            var commitRange = $"{from}...{to}";

            //https://github.com/bilal-fazlani/commanddotnet/compare/CommandDotNet_3.5.0...CommandDotNet_3.5.1
            _writeln($"{_repo.GetOriginRepoUrl().HttpsUrl}/compare/{commitRange}".Theme_GitLinks());

            if (args.ShowCommits)
            {
                _writeln($"git log --graph {commitRange} -- {project.Directory}".Theme_GitLinks());
                _writeln($"git log --oneline --graph {commitRange} -- {project.Directory}".Theme_GitLinks());

                foreach (var commit in project
                    .GetCommitsBetween(target, nextTarget, cancellationToken)
                    .TakeUntil(_ => cancellationToken.IsCancellationRequested))
                {
                    _writeln(
                        $"{commit.ShortSha()} {commit.Author.Theme_WhenDateTime()} {commit.MessageShort.Theme_GitMessage()}");
                }
            }

            if (args.ShowFiles)
            {
                // https://stackoverflow.com/questions/1552340/how-to-list-only-the-file-names-that-changed-between-two-commits/6827937

                _writeln($"git diff {commitRange} -- {project.Directory}".Theme_GitLinks());
                _writeln($"git diff --name-status {commitRange} -- {project.Directory}".Theme_GitLinks());

                foreach (var file in project.GetFilesChangedBetween(target, nextTarget))
                {
                    _writeln($"{file.Theme_FileChange()}");
                }
            }
        }
    }
}
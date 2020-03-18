using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Rendering;
using MoreLinq.Extensions;
using Pastel;

namespace mrGitTags
{
    public class RepoApp
    {
        private Repo _repo;
        private IConsole _console;

        public Task<int> Intercept(
            InterceptorExecutionDelegate next, 
            RepoAppOptions options,
            IConsole console)
        {
            _console = console;
            _repo = new Repo(options.RepoDirectory);
            return next();
        }

        [Command(Description = "list the projects in the repo")]
        public void list()
        {
            _repo.GetProjects().ForEach(p => 
                _console.Out.WriteLine($"{$"#{p.Index}".PadLeft(2)} {p.Name.Theme_ProjectName()}"));
        }

        [Command(Description = "list all tags for the given project(s)")]
        public void tags(
            ProjectsOptions projectsOptions, 
            [Option(ShortName = "i", LongName = "include-prereleases")] bool includePrereleases)
        {
            foreach (var project in _repo.GetProjects(projectsOptions))
            {
                _console.Out.WriteLine($"{$"#{project.Index}".PadLeft(2)} {project.Name.Theme_ProjectName()}");
                foreach (var tagInfo in _repo.GetTagsOrEmpty(project.Name).Where(t => !t.IsPrerelease || includePrereleases))
                {
                    _console.Out.WriteLine($"   {tagInfo.FriendlyName.Theme_GitNameAlt()}  {tagInfo.ShortSha}");
                }
            }
        }

        [Command(Description = "create and push a tag for the next version of the project")]
        public void increment(
            [Operand(Name = "project", Description = "The id or name of the project")] string projectKey,
            [Option(ShortName = "t", LongName = "type")] SemVerElement element = SemVerElement.patch)
        {
            var project = _repo.GetProjectOrDefault(projectKey) ?? throw new ArgumentOutOfRangeException("project", $"unknown project:{projectKey}");
            var nextTag = project.Increment(element);
            _console.Out.WriteLine($"added {nextTag.FriendlyName}");
            _console.Out.WriteLine("run the following command to push the tag to the remote");
            _console.Out.WriteLine();
            _console.Out.WriteLine($"git push origin {nextTag.FriendlyName}".Theme_GitLinks());
        }

        [Command(Description = "Status each project for since the last tag of each project to the head of the current branch.")]
        public void status(
            ProjectsOptions projectsOptions,
            [Option(ShortName = "s", LongName = "summary-only", Description = "list only the project and change summary.")] bool summaryOnly = false,
            [Option(ShortName = "f", LongName = "show-files", Description = "list all files changed within each project")] bool showFiles = false)
        {
            foreach (var project in _repo.GetProjects(projectsOptions))
            {
                if (project.LatestTag == null)
                {
                    _console.Out.WriteLine($"{project.Name.Theme_ProjectName()}: {"no tag".Pastel(Color.Orange)}");
                    continue;
                }

                var taggedCommit = project.LatestTaggedCommit;
                var headCommit = _repo.Git.Head.Tip;

                var changes = project.GetTreeChanges(headCommit);

                _console.Out.WriteLine($"{$"#{project.Index}".PadLeft(2)} {project.Name.Theme_ProjectName()}: {changes.Summary()}");
                if (!summaryOnly)
                {
                    _console.Out.WriteLine($"  branch: {_repo.Git.Head.FriendlyName.Theme_GitName()} " +
                                          $"{headCommit.Author.Name.Theme_Person()} {headCommit.Committer.When.Theme_Date()}");
                    _console.Out.WriteLine($"          {headCommit.MessageShort}");
                    _console.Out.WriteLine($"  tag   : {project.LatestTag.Tag.FriendlyName.Theme_GitName()} " +
                                          $"{taggedCommit.Author.Name.Theme_Person()} {taggedCommit.Committer.When.Theme_Date()}");
                    _console.Out.WriteLine($"          {taggedCommit.MessageShort}");
                    if (changes.Count > 0)
                    {
                        _console.Out.WriteLine($" commits: {$"git log --oneline {project.LatestTag.ShortSha}^..HEAD -- {project.Directory}".Theme_GitLinks()}");
                    }
                    if (showFiles && changes.Count > 0)
                    {
                        _console.Out.WriteLine("  changes:");
                        foreach (var change in changes)
                        {
                            var path = change.Path == change.OldPath ? change.Path : $"{change.OldPath} > {change.Path}";
                            var color = change.Status.Theme_Change();
                            _console.Out.WriteLine($"    {change.Status.ToString().PadLeft(11)} : {path}".Pastel(color));
                        }
                    }
                }
            }
        }
    }
}
using System.Drawing;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Rendering;
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

        public void list()
        {
            var projects = _repo.GetProjects();

            for (int i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                _console.Out.WriteLine(
                    $"{$"#{i}".PadLeft(2)} {project.Name.Theme_ProjectName()}");
            }
        }

        [Command(Description = "Status each project for since the last tag of each project to the head of the current branch.")]
        public void status(
            [Option(ShortName = "i", LongName = "project-index", Description = "show details only for the given project index")] int? projectIndex = null,
            [Option(ShortName = "s", LongName = "summary-only", Description = "list only the project and change summary.")] bool summaryOnly = false,
            [Option(ShortName = "f", LongName = "show-files", Description = "list all files changed within each project")] bool showFiles = false)
        {
            var projects = _repo.GetProjects();

            for (int i = 0; i < projects.Count; i++)
            {
                if (projectIndex.HasValue && i != projectIndex.Value)
                {
                    continue;
                }

                var project = projects[i];

                if (project.LatestTag == null)
                {
                    _console.Out.WriteLine($"{project.Name.Theme_ProjectName()}: {"no tag".Pastel(Color.Orange)}");
                    continue;
                }

                var taggedCommit = project.LatestTaggedCommit;
                var headCommit = _repo.Git.Head.Tip;

                var changes = project.GetTreeChanges(headCommit);

                _console.Out.WriteLine($"{$"#{i}".PadLeft(2)} {project.Name.Theme_ProjectName()}: {changes.Summary()}");
                if (!summaryOnly)
                {
                    _console.Out.WriteLine($"  branch: {_repo.Git.Head.FriendlyName.Theme_GitName()} " +
                                          $"{headCommit.Author.Name.Theme_Person()} {headCommit.Committer.When.Theme_Date()}");
                    _console.Out.WriteLine($"          {headCommit.MessageShort}");
                    _console.Out.WriteLine($"  tag   : {project.LatestTag.Tag.FriendlyName.Theme_GitName()} " +
                                          $"{taggedCommit.Author.Name.Theme_Person()} {taggedCommit.Committer.When.Theme_Date()}");
                    _console.Out.WriteLine($"          {taggedCommit.MessageShort}");
                    _console.Out.WriteLine($" commits: git log --oneline {project.LatestTag.Tag.Target.ShortSha()}..HEAD -- {project.Directory}");
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
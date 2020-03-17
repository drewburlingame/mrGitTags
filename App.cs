using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CommandDotNet;
using CommandDotNet.Rendering;
using LibGit2Sharp;
using MoreLinq;
using Pastel;
using Semver;

namespace mrGitTags
{
    public class App
    {
        [Command(Description = "Status each project for since the last tag of each project to the head of the current branch.")]
        public void status(
            IConsole console, 
            string directory = null,
            [Option(ShortName = "v", LongName = "verbose", Description = "list all files changed within each project")] bool verbose = false,
            [Option(ShortName = "q", LongName = "quite", Description = "list only the project and change summary.")] bool quite = false)
        {
            var repo = new Repo(directory);

            var projects = GetProjects(repo);

            foreach (var project in projects)
            {
                if (project.TagInfo == null)
                {
                    console.Out.WriteLine($"{project.Name.Theme_ProjectName()}: {"no tag".Pastel(Color.Orange)}");
                    continue;
                }

                var taggedCommit = repo.Git.Lookup<Commit>(project.TagInfo.Tag.Target.Sha);
                var headCommit = repo.Git.Head.Tip;

                var treeChanges = repo.Git.Diff.Compare<TreeChanges>(taggedCommit.Tree, headCommit.Tree);

                var changes = treeChanges
                    .Where(c => c.Status.HasSemanticMeaning())
                    .Where(c =>
                        c.Path.StartsWith(project.Directory)
                        || c.OldPath.StartsWith(project.Directory))
                    .ToList();

                console.Out.WriteLine($"{project.Name.Theme_ProjectName()}: {changes.Summary()}");
                if (!quite)
                {
                    console.Out.WriteLine($"  branch: {repo.Git.Head.FriendlyName.Theme_GitName()} " +
                                          $"{headCommit.Author.Name.Theme_Person()} {headCommit.Committer.When.Theme_Date()}");
                    console.Out.WriteLine($"          {headCommit.MessageShort}");
                    console.Out.WriteLine($"  tag   : {project.TagInfo.Tag.FriendlyName.Theme_GitName()} " +
                                          $"{taggedCommit.Author.Name.Theme_Person()} {taggedCommit.Committer.When.Theme_Date()}");
                    console.Out.WriteLine($"          {taggedCommit.MessageShort}");
                    if (verbose && changes.Count > 0)
                    {
                        console.Out.WriteLine("  changes:");
                        foreach (var change in changes)
                        {
                            var path = change.Path == change.OldPath ? change.Path : $"{change.OldPath} > {change.Path}";
                            var color = change.Status.Theme_Change();
                            console.Out.WriteLine($"    {change.Status.ToString().PadLeft(11)} : {path}".Pastel(color));
                        }
                    }
                }
            }
        }

        private static Dictionary<string, ProjectInfo>.ValueCollection GetProjects(Repo repo)
        {
            var projectsByName = Directory
                .EnumerateFiles(repo.Dir, "*.*proj", SearchOption.AllDirectories)
                .Select(p => p.Remove(0, repo.Dir.Length+1))
                .Select(ProjectInfo.ParseOrDefault)
                .Where(p => p != null 
                            && !p.Name.EndsWith("Tests") 
                            && !p.Name.EndsWith("Example"))
                .ToDictionary(p => p.Name);

            repo.Git.Tags
                .Select(TagInfo.ParseOrDefault)
                .Where(tag => tag != null && projectsByName.ContainsKey(tag.Name))
                .GroupBy(tag => tag.Name)
                .ForEach(tags => projectsByName[tags.Key].TagInfo = tags.OrderByDescending<TagInfo, SemVersion>(t => t.SemVersion).First());

            return projectsByName.Values;
        }
    }
}
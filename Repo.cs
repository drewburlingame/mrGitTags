using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MoreLinq;
using static System.IO.Directory;

namespace mrGitTags
{
    public class Repo
    {
        private List<Project>? _projects;
        private Dictionary<string, List<TagInfo>>? _tagsByProject;

        public string Dir { get; }
        public Repository Git { get; }

        public Branch Branch { get; }
        public Commit Tip => Branch.Tip;
        public Commit FirstCommit => Git.Commits.Last();

        public Repo(string directory, string branch)
        {
            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            directory ??= GetCurrentDirectory();
            directory = directory.Trim('/', '\\');

            Dir = directory;
            Git = new Repository(directory);
            Branch = branch == "current" || branch == "!" 
                ? Git.Head 
                : Git.Branches[branch];
        }

        internal IList<Project> GetProjects()
        {
            return EnsureProjects().ToList();
        }

        internal IList<Project> GetProjects(ProjectsOperand projectsOperand)
        {
            if (!projectsOperand.Projects.Any())
            {
                return GetProjects();
            }

            return EnsureProjects()
                .Where(p => 
                    projectsOperand.Projects.Any(pk => IsKeyFor(pk, p)))
                .ToList();
        }

        internal Project? GetProjectOrDefault(string projectKey)
        {
            return EnsureProjects()
                .SingleOrDefault(p => IsKeyFor(projectKey, p));
        }

        private static bool IsKeyFor(string projectKey, Project project)
        {
            return BuildKeys(project).Any(k => k == projectKey);
        }

        private static IEnumerable<string> BuildKeys(Project project)
        {
            yield return project.Name;
            yield return project.Index.ToString();
            yield return $"#{project.Index}";
        }

        public List<TagInfo> GetTagsOrEmpty(string projectName) => 
            EnsureTags().GetValueOrDefault(projectName) ?? new List<TagInfo>();

        public RepoUrl GetOriginRepoUrl()
        {
            return new RepoUrl(Git.Network.Remotes["origin"].Url);
        }

        private Dictionary<string, List<TagInfo>> EnsureTags()
        {
            if (_tagsByProject == null)
            {
                _tagsByProject = Git.Tags
                    .Select(tag => TagInfo.ParseOrDefault(this, tag))
                    .Where(tag => tag is not null)
                    .Cast<TagInfo>()
                    .GroupBy(tag => tag.Name)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var tags = g.OrderByDescending(t => t.SemVersion).ToList();
                            for (int i = 0; i < tags.Count; i++)
                            {
                                if(i-1 >= 0)
                                    tags[i - 1].Previous = tags[i];
                                if (i+1 < tags.Count)
                                    tags[i + 1].Next = tags[i];
                            }
                            return tags;
                        });
            }

            return _tagsByProject;
        }

        private IList<Project> EnsureProjects()
        {
            if (_projects == null)
            {
                _projects = EnumerateFiles(Dir, "*.*proj", SearchOption.AllDirectories)
                    .Select(p => p.Remove(0, Dir.Length + 1))
                    .Select(s => new Project(this, s))
                    .Where(p => !p.Name.EndsWith("Tests")
                                && !p.Name.EndsWith("Example"))
                    .OrderBy(p => p.Name)
                    .ToList();

                _projects.ForEach((project, i) => project.Index = i);
            }

            return _projects;
        }
    }
}
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
        private List<Project> _projects;
        private Dictionary<string, List<TagInfo>> _tagsByProject;

        public string Dir { get; }
        public Repository Git { get; }

        public Repo(string directory)
        {
            directory ??= GetCurrentDirectory();
            directory = directory.Trim('/', '\\');

            Dir = directory;
            Git = new Repository(directory);
        }

        internal IList<Project> GetProjects()
        {
            return EnsureProjects().ToList();
        }

        internal IList<Project> GetProjects(ProjectsOptions projectsOptions)
        {
            if (!projectsOptions.Projects.Any())
            {
                return GetProjects();
            }

            return EnsureProjects()
                .Where(p => BuildKeys(p).Any(k => 
                    projectsOptions.Projects.Any(pk => pk == k)))
                .ToList();
        }

        internal IList<Project> GetProjects(ProjectOptions projectOptions)
        {
            if (string.IsNullOrWhiteSpace(projectOptions.Project))
            {
                return GetProjects();
            }

            return EnsureProjects()
                .Where(p => BuildKeys(p).Any(k => k == projectOptions.Project))
                .ToList();
        }

        private static IEnumerable<string> BuildKeys(Project project)
        {
            yield return project.Name;
            yield return project.Index.ToString();
            yield return $"#{project.Index}";
        }

        public List<TagInfo> GetTagsOrEmpty(string projectName) => 
            EnsureTags().GetValueOrDefault(projectName) ?? new List<TagInfo>();

        public bool TryGetTags(string projectName, out List<TagInfo> tagInfos) => 
            EnsureTags().TryGetValue(projectName, out tagInfos);

        private Dictionary<string, List<TagInfo>> EnsureTags()
        {
            if (_tagsByProject == null)
            {
                _tagsByProject = Git.Tags
                    .Select(TagInfo.ParseOrDefault)
                    .Where(tag => tag != null)
                    .GroupBy(tag => tag.Name)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(t => t.SemVersion).ToList());
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
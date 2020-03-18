using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace mrGitTags
{
    public class Project
    {
        private readonly Repo _repo;
        private readonly List<TagInfo> _tags;

        private Commit _latestTaggedCommit;

        public string Name { get; }
        public string ProjectFile { get; }
        public string Directory { get; }

        public TagInfo LatestTag => _tags.FirstOrDefault();

        public Commit HeadCommit => _repo.Git.Head.Tip;

        public Commit LatestTaggedCommit => _latestTaggedCommit
            ??= LatestTag == null
                ? null
                : _repo.Git.Lookup<Commit>(LatestTag.Tag.Target.Sha);

        public Project(Repo repo, string projectFile)
            : this(repo,
                Path.GetFileNameWithoutExtension(projectFile),
                projectFile,
                $"{Path.GetDirectoryName(projectFile)}/")
        {
        }

        private Project(Repo repo, string name, string projectFile, string directory)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));

            _tags = repo.GetTagsOrEmpty(name);
        }

        public ICollection<TreeEntryChanges> GetTreeChanges(Commit latestCommit = null)
        {
            if (LatestTaggedCommit == null)
            {
                return new List<TreeEntryChanges>();
            }
            latestCommit ??= _repo.Git.Head.Tip;
            var changes = _repo.Git.Diff.Compare<TreeChanges>(LatestTaggedCommit.Tree, latestCommit.Tree);
            return changes
                .Where(c => c.Status.HasSemanticMeaning())
                .Where(c =>
                    c.Path.StartsWith(Directory)
                    || c.OldPath.StartsWith(Directory))
                .ToList();
        }

        public ICollection<PatchEntryChanges> GetPatchChanges(Commit latestCommit)
        {
            if (LatestTaggedCommit == null)
            {
                return new List<PatchEntryChanges>();
            }
            var changes = _repo.Git.Diff.Compare<Patch>(LatestTaggedCommit.Tree, latestCommit.Tree);
            return changes
                .Where(c => c.Status.HasSemanticMeaning())
                .Where(c =>
                    c.Path.StartsWith(Directory)
                    || c.OldPath.StartsWith(Directory))
                .ToList();
        }
    }
}
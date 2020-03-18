using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Semver;

namespace mrGitTags
{
    public class Project
    {
        private readonly Repo _repo;
        private readonly List<TagInfo> _tags;

        private Commit _latestTaggedCommit;

        public int Index { get; set; }
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

        public TagInfo Increment(SemVerElement element)
        {
            var nextVersion = Increment(LatestTag, element);
            var newTag = _repo.Git.ApplyTag($"{Name}_{nextVersion}", HeadCommit.Sha);
            var newTagInfo = TagInfo.ParseOrDefault(newTag);
            _tags.Insert(0, newTagInfo);
            return newTagInfo;
        }

        private SemVersion Increment(TagInfo tagInfo, SemVerElement type)
        {
            var semver = tagInfo.SemVersion;
            switch (type)
            {
                case SemVerElement.major:
                    return semver.Change(major: semver.Major + 1, minor: 0, patch: 0, prerelease: null);
                case SemVerElement.minor:
                    return semver.Change(minor: semver.Minor + 1, patch: 0, prerelease: null);
                case SemVerElement.patch:
                    return semver.Change(patch: semver.Patch + 1, prerelease: null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
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
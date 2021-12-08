using System;
using LibGit2Sharp;
using Semver;

namespace mrGitTags
{
    public class TagInfo
    {
        private readonly Repo _repo;
        private Commit? _commit;

        public string Name { get; }
        public SemVersion SemVersion { get; }
        public Tag Tag { get; }

        public string FriendlyName => $"{Name}_{SemVersion}";

        public bool IsPrerelease => !string.IsNullOrWhiteSpace(SemVersion.Prerelease);

        public string ShortSha => Tag.Target.ShortSha();

        public Commit Commit => _commit ??= _repo.Git.Lookup<Commit>(Tag.Target.Sha);

        public TagInfo? Previous { get; set; }
        public TagInfo? Next { get; set; }

        private TagInfo(Repo repo, string name, SemVersion semVersion, Tag tag)
        {
            _repo = repo;
            Name = name;
            SemVersion = semVersion;
            Tag = tag;
        }

        public static TagInfo ParseOrThrow(Repo repo, Tag tag)
        {
            return ParseOrDefault(repo, tag) ??
                   throw new InvalidOperationException($"Unable to parse tag: {tag}");
        }

        public static TagInfo? ParseOrDefault(Repo repo, Tag tag)
        {
            var parts = tag.FriendlyName.Split("_");
            if (parts.Length != 2)
            {
                return null;
            }

            var name = parts[0];
            if (!SemVersion.TryParse(parts[1], out var semver))
            {
                return null;
            }

            if (tag.PeeledTarget is not Commit c)
            {
                return null;
            }

            return new TagInfo(repo, name, semver, tag);
        }

        public override string ToString() => $"{nameof(TagInfo)}: {FriendlyName}";
    }
}
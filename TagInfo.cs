using LibGit2Sharp;
using Semver;

namespace mrGitTags
{
    public class TagInfo
    {
        public string Name { get; }
        public SemVersion SemVersion { get; }
        public Tag Tag { get; }

        private TagInfo(string name, SemVersion semVersion, Tag tag)
        {
            Name = name;
            SemVersion = semVersion;
            Tag = tag;
        }

        public static TagInfo ParseOrDefault(Tag tag)
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

            return new TagInfo(name, semver, tag);
        }
    }
}
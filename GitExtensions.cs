using LibGit2Sharp;

namespace mrGitTags
{
    public static class GitExtensions
    {
        public static string ShortSha(this GitObject obj) => obj.Sha.ShortSha();
        public static string ShortSha(this string sha) => sha.Substring(0,7);
    }
}
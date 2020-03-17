using System.IO;
using LibGit2Sharp;

namespace mrGitTags
{
    public class Repo
    {
        public string Dir { get; }
        public Repository Git { get; }

        public Repo(string directory)
        {
            directory ??= Directory.GetCurrentDirectory();
            directory = directory.Trim('/', '\\');

            Dir = directory;
            Git = new Repository(directory);
        }
    }
}
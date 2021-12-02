using System.IO;
using CommandDotNet;

namespace mrGitTags
{
    public class RepoAppOptions : IArgumentModel
    {
        private Repo _repo = new(Directory.GetCurrentDirectory(), "current");

        [Option]
        public string RepoDir
        {
            get => _repo.Dir;
            set => _repo = new Repo(value, _repo.Branch.FriendlyName);
        }

        [Option('b',
            Description = "The branch to use as head")]
        public string Branch
        {
            get => _repo.Branch.FriendlyName; 
            set => _repo = new Repo(_repo.Dir, value);
        }

        public Repo Repo() => _repo;
    }
}
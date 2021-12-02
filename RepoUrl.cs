using System.Linq;

namespace mrGitTags
{
    public class RepoUrl
    {
        public string? HttpsUrl { get; }
        public string? RemoteProjectPath { get; }

        public RepoUrl(string repoOriginUrl)
        {
            RemoteProjectPath = GetProjectNameFromRemoteUrl(repoOriginUrl);
            if (RemoteProjectPath is not null)
            {
                HttpsUrl = GetHttpsUrl(repoOriginUrl, RemoteProjectPath);
            }
        }

        private static string? GetHttpsUrl(string repoOriginUrl, string projectName)
        {
            var isGitHub = repoOriginUrl.Contains("github.com");
            return isGitHub
                ? $"https://github.com/{projectName}"
                : null;
        }

        private static string? GetProjectNameFromRemoteUrl(string repoOriginUrl)
        {
            var isSsh = repoOriginUrl.StartsWith("git@");
            if (isSsh)
            {
                var parts = repoOriginUrl.Split(":");
                var projectName = parts.Last();
                return projectName.Substring(0, projectName.Length - ".git".Length);
            }
            return null;
        }
    }
}
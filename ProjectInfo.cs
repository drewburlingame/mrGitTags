using System.IO;

namespace mrGitTags
{
    public class ProjectInfo
    {
        public string Name { get; }
        public string ProjectFile { get; }
        public string Directory { get; }

        public TagInfo TagInfo { get; set; }

        public ProjectInfo(string name, string projectFile, string directory)
        {
            Name = name;
            ProjectFile = projectFile;
            Directory = directory;
        }

        public static ProjectInfo ParseOrDefault(string projectFile)
        {
            return new ProjectInfo(
                Path.GetFileNameWithoutExtension(projectFile),
                projectFile,
                $"{Path.GetDirectoryName(projectFile)}/");
        }
    }
}
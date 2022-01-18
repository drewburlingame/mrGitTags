using System.Collections.Generic;
using System.IO;

namespace mrGitTags
{
    public class Config
    {
        private static readonly string ConfigFolder = ".mrGitTags";
        private static string Path = System.IO.Path.Join(ConfigFolder, "config.json");

        private static Config? Instance;

        public Dictionary<string, string> SkippedCommitsByProject { get; set; } = new();

        public void Save()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(this);
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
            File.WriteAllText(Path, json);
        }

        public static Config Get() => Instance ??= LoadFromFile() ?? new Config();

        private static Config? LoadFromFile()
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path);
                return System.Text.Json.JsonSerializer.Deserialize<Config>(json);
            }
            return null;
        }
    }
}
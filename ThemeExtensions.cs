using System.Drawing;
using LibGit2Sharp;
using Pastel;

namespace mrGitTags
{
    public static class ThemeExtensions
    {
        public static string Theme_ProjectName(this object text) => text?.ToString().Pastel(Color.Cyan);
        public static string Theme_GitName(this object text) => text?.ToString().Pastel(Color.Cyan);
        public static string Theme_Person(this object text) => text?.ToString().Pastel(Color.DarkCyan);
        public static string Theme_Date(this object text) => text?.ToString().Pastel(Color.DeepSkyBlue);

        public static string Theme_Change(this int changeCount, ChangeKind changeKind)
        {
            return changeCount == 0
                ? changeCount.ToString()
                : changeCount.ToString().Pastel(changeKind.Theme_Change());
        }

        public static Color Theme_Change(this ChangeKind changeKind)
        {
            switch (changeKind)
            {
                case ChangeKind.Added:
                case ChangeKind.Copied:
                    return Color.Green;
                case ChangeKind.Deleted:
                    return Color.Red;
                default:
                    return Color.Yellow;
            }
        }
    }
}
using System;
using System.Drawing;
using LibGit2Sharp;
using Pastel;

namespace mrGitTags
{
    public static class ThemeExtensions
    {
        public static string Theme_ProjectIndexAndName(this Project project) =>
            $"{$"#{project.Index}",2} {project.Name}".Pastel(Color.Cyan);

        public static string? Theme_GitName(this string? text) => text?.Pastel(Color.Cyan);
        public static string? Theme_GitNameAlt(this string? text) => text?.Pastel(Color.GhostWhite);
        public static string? Theme_GitLinks(this string? text) => text?.Pastel(Color.Violet);
        public static string Theme_GitMessage(this string text) => text.Pastel(text.StartsWith("Merge") ? Color.Yellow : Color.White);

        public static string Theme_Name(this Signature signature) => signature.Name.Pastel(Color.DarkCyan);
        public static string Theme_WhenDate(this Signature signature) => signature.When.Theme_Date();
        public static string Theme_WhenDateTime(this Signature signature) => signature.When.Theme_DateTime();
        private static string Theme_Date(this DateTimeOffset dto) => dto.ToString("yyyy/MM/dd").Pastel(Color.DeepSkyBlue);
        private static string Theme_DateTime(this DateTimeOffset dto) => dto.ToString("yyyy/MM/dd HH:mm:SS").Pastel(Color.DeepSkyBlue);

        public static string Theme_FileChange(this TreeEntryChanges change)
        {
            var path = change.Path == change.OldPath ? change.Path : $"{change.OldPath} > {change.Path}";
            var color = change.Status.Theme_Change();
            return $"{change.Status.ToString().PadLeft(11)} : {path}".Pastel(color);
        }

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
                    return Color.LightGreen;
                case ChangeKind.Deleted:
                    return Color.Red;
                default:
                    return Color.Yellow;
            }
        }
    }
}
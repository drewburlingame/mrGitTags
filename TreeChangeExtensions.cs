using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibGit2Sharp;

namespace mrGitTags
{
    public static class TreeChangeExtensions
    {
        public static string Summary(this TreeChanges changes)
        {
            // copied from private TreeChanges.DebuggerDisplay
            return string.Format(CultureInfo.InvariantCulture,
                "+{0} ~{1} -{2} \u00B1{3} R{4} C{5}",
                changes.Added.Count().Theme_Change(ChangeKind.Added),
                changes.Modified.Count().Theme_Change(ChangeKind.Modified),
                changes.Deleted.Count().Theme_Change(ChangeKind.Deleted),
                changes.TypeChanged.Count().Theme_Change(ChangeKind.TypeChanged),
                changes.Renamed.Count().Theme_Change(ChangeKind.Renamed),
                changes.Copied.Count().Theme_Change(ChangeKind.Copied));
        }

        public static string Summary(this IEnumerable<TreeEntryChanges> changes)
        {
            int added = 0, copied = 0, deleted = 0, modified = 0, renamed = 0, typeChanged = 0;
            foreach (var change in changes)
            {
                switch (change.Status)
                {
                    case ChangeKind.Added:
                        added++;
                        break;
                    case ChangeKind.Copied:
                        copied++;
                        break;
                    case ChangeKind.Deleted:
                        deleted++;
                        break;
                    case ChangeKind.Modified:
                        modified++;
                        break;
                    case ChangeKind.Renamed:
                        renamed++;
                        break;
                    case ChangeKind.TypeChanged:
                        typeChanged++;
                        break;
                }
            }

            // copied from private TreeChanges.DebuggerDisplay
            return string.Format(CultureInfo.InvariantCulture,
                "+{0} ~{1} -{2} \u00B1{3} R{4} C{5}",
                added.Theme_Change(ChangeKind.Added),
                modified.Theme_Change(ChangeKind.Modified),
                deleted.Theme_Change(ChangeKind.Deleted),
                typeChanged.Theme_Change(ChangeKind.TypeChanged),
                renamed.Theme_Change(ChangeKind.Renamed),
                copied.Theme_Change(ChangeKind.Copied));
        }

        public static bool HasSemanticMeaning(this ChangeKind changeKind)
        {
            switch (changeKind)
            {
                case ChangeKind.Added:
                case ChangeKind.Copied:
                case ChangeKind.Deleted:
                case ChangeKind.Modified:
                case ChangeKind.Renamed:
                case ChangeKind.TypeChanged:
                    return true;
                default:
                    return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.ProgramsHelper.Models
{
    public class Program : IEquatable<Program>
    {
        public string Path { get; set; }
        public string Arguments { get; set; }
        public string Icon { get; set; }
        public int IconIndex { get; set; }
        public string WorkDir { get; set; }
        public string Name { get; set; }
        public string AppId { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Program other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(Path, other.Path) &&
                   string.Equals(Arguments, other.Arguments) &&
                   string.Equals(Icon, other.Icon) &&
                   IconIndex == other.IconIndex &&
                   string.Equals(WorkDir, other.WorkDir) &&
                   string.Equals(Name, other.Name) &&
                   string.Equals(AppId, other.AppId);
        }

        public override bool Equals(object obj)
        {
            if (obj is Program otherProgram)
            {
                return Equals(otherProgram);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (Path?.GetHashCode() ?? 0) ^
                   (Arguments?.GetHashCode() ?? 0) ^
                   (Icon?.GetHashCode() ?? 0) ^
                   IconIndex.GetHashCode() ^
                   (WorkDir?.GetHashCode() ?? 0) ^
                   (Name?.GetHashCode() ?? 0) ^
                   (AppId?.GetHashCode() ?? 0);
        }

        public static bool operator ==(Program left, Program right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Program left, Program right)
        {
            return !(left == right);
        }
    }

}
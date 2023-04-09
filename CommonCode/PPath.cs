using System.Diagnostics;
using System.IO;

namespace CommonCode
{
    public class PPath
    {
        private readonly string myPath;

        [DebuggerStepThrough]
        private PPath(string path)
        {
            myPath = path;
        }

        [DebuggerStepThrough]
        public static implicit operator PPath (string path)
        {
            return new PPath(path);
        }

        [DebuggerStepThrough]
        public static implicit operator string (PPath path)
        {
            return path.myPath;
        }

        [DebuggerStepThrough]
        public static PPath operator /(PPath p1, PPath p2)
        {
            return string.IsNullOrEmpty(p2) ? p1 : (PPath)Path.Combine(p1, p2);
        }

        public PPath FullPath
        {
            [DebuggerStepThrough]
            get { return Path.GetFullPath(myPath); }
        }

        [DebuggerStepThrough]
        public bool Equals(PPath other)
        {
            return myPath.Equals(other.myPath);
        }

        public static PPath AddSlash(PPath src)
        {
            if (((string)src).EndsWith("\\"))
            {
                return src;
            }

            return src + "\\";
        }

        public override string ToString()
        {
            return this;
        }
    }
}
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CommonCode
{
    public class PPath
    {
        private readonly string _path;

        [DebuggerStepThrough]
        private PPath(string path)
        {
            _path = path;
        }

        [DebuggerStepThrough]
        public static implicit operator PPath (string path)
        {
            return new PPath(path);
        }

        [DebuggerStepThrough]
        public static implicit operator string (PPath path)
        {
            return path._path;
        }

        [DebuggerStepThrough]
        public static PPath operator /(PPath p1, PPath p2)
        {
            return string.IsNullOrEmpty(p2) ? p1 : (PPath)Path.Combine(p1, p2);
        }

        public static PPath GetExeDirectory()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);

            string directoryName = Path.GetDirectoryName(assembly.Location);
            Debug.Assert(directoryName != null);

            return ((PPath)directoryName).FullPath;
        }

        public PPath FullPath
        {
            [DebuggerStepThrough]
            get { return Path.GetFullPath(_path); }
        }

        [DebuggerStepThrough]
        public bool Equals(PPath other)
        {
            return _path.Equals(other._path);
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
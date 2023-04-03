using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CommonCode
{
    public static class PatContainer
    {
        public static readonly string PersonalAccessToken;

        static PatContainer()
        {
            PersonalAccessToken = GetPat();
        }

        private static string GetPat()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);

            string directoryName = Path.GetDirectoryName(assembly.Location);
            Debug.Assert(directoryName != null);

            return File.ReadAllText(Path.Combine(directoryName, "PAT.txt"));
        }
    }
}
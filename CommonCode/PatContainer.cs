using System.IO;

namespace CommonCode
{
    public static class PatContainer
    {
        public static readonly string PersonalAccessToken;

        static PatContainer()
        {
            PPath directory = PPath.GetExeDirectory();

            PersonalAccessToken = File.ReadAllText(directory / "PAT.txt");
        }
    }
}
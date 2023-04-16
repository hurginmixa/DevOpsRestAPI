using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using CommonCode;

namespace ItemsReport
{
    public class Config
    {
        public string Token => PatContainer.PersonalAccessToken;

        public int[] Ids { get; set; } = Array.Empty<int>();

        public string OutputFile { get; set; }

        public string[] SelectedBranchPaths { get; set; } = Array.Empty<string>();

        public static Config GetConfig()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);

            string directoryName = Path.GetDirectoryName(assembly.Location);
            Debug.Assert(directoryName != null);

            string json = File.ReadAllText(Path.Combine(directoryName, "Config.json"));

            return JsonSerializer.Deserialize<Config>(json);
        }
    }
}

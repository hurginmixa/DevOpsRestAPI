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
        public class FilterClass
        {
            public string[] SelectedBranchPaths { get; set; } = Array.Empty<string>();

            public DateTime StartDate { get; set; } = DateTime.MinValue;

            public DateTime EndDate { get; set; } = DateTime.MaxValue;

            public bool IsFiltered
            {
                get
                {
                    if (StartDate != DateTime.MinValue)
                    {
                        return true;
                    }

                    if (EndDate != DateTime.MaxValue)
                    {
                        return true;
                    }

                    if (SelectedBranchPaths.Length != 0)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public string Token => PatContainer.PersonalAccessToken;

        public int[] Ids { get; set; } = Array.Empty<int>();

        public string OutputFile { get; set; }

        public FilterClass Filter { get; set; } = new FilterClass();

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

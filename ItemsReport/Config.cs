﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommonCode;

namespace ItemsReport
{
    public class Config
    {
        #region public class FilterClass

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

        #endregion

        public string Token => PatContainer.PersonalAccessToken;

        public string HTMLTitle { get; set; } = "TFS Report";

        [JsonPropertyName("Old Ids")]
        public int[] OldIds { get; set; } = Array.Empty<int>();

        public int[] Ids { get; set; } = Array.Empty<int>();

        public string OutputFile { get; set; }

        public string CacheDataFile { get; set; }

        public FilterClass Filter { get; set; } = new FilterClass();

        public static Config GetConfig(string configFileName)
        {
            string configFilePath = PPath.GetExeDirectory() / configFileName;

            Console.WriteLine($"configFilePath: {configFilePath}");

            string json = File.ReadAllText(configFilePath);

            return JsonSerializer.Deserialize<Config>(json);
        }
    }
}

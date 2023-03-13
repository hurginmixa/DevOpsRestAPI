using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceProfileSample
{
    internal class Program
    {
        private static readonly string PersonalAccessToken = GetPat();
        private const string Organization = "AzCamtek";
        private const string Project = "Falcon";

        private static string GetPat()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);
            
            Uri uri = new Uri(assembly.CodeBase);

            string directoryName = Path.GetDirectoryName(uri.AbsolutePath);
            Debug.Assert(directoryName != null);

            return File.ReadAllText(Path.Combine(directoryName, "PAT.txt"));
        }

        static void Main()
        {
            var workItems = new int[]
            {
                39862, 34598, 39309, 39559, 39378, 39347, 39523, 39346, 39319, 39967, 39926, 39862, 35781, 39346, 38358,
                38280, 37192, 38836, 37787
            };

            using (TextWriter tw = new StreamWriter(@"c:\temp\mixa.csv"))
            {
                PerformWorkItems(workItems, tw, 0);
            }

            Console.WriteLine("Complete");
        }

        private static void PerformWorkItems(IEnumerable<int> workItems, TextWriter textWriter, int level)
        {
            string responseBody = GetWorkItemListByIds(workItems.OrderBy(rr => rr)).Result;
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(responseBody);

            foreach (GitWorkItem gitWorkItem in gitWorkItemList.Value)
            {
                Console.WriteLine($"{gitWorkItem.Id} {gitWorkItem.Fields.Title}");

                List<int> childList = new List<int>();
                List<int> pullRequestList = new List<int>();

                foreach (GitRelation relation in gitWorkItem.Relations)
                {
                    if (relation.Attributes.Name == "Child" || relation.Attributes.Name == "Pull Request")
                    {
                        string decodedUrl = Uri.UnescapeDataString(relation.Url);
                        string txtId = decodedUrl.Substring(decodedUrl.LastIndexOf('/') + 1);

                        switch (relation.Attributes.Name)
                        {
                            case "Child":
                                childList.Add(int.Parse(txtId));
                                break;

                            case "Pull Request":
                                pullRequestList.Add(int.Parse(txtId));
                                break;
                        }
                    }
                }

                textWriter.Write(new string('\t', level));
                textWriter.Write($"{gitWorkItem.Id},");
                textWriter.Write($"{gitWorkItem.Fields.WorkItemType},");
                textWriter.Write($"{gitWorkItem.Fields.State},");
                textWriter.Write($"\"{gitWorkItem.Fields.Title.Replace("\"", "'")}\"");

                if (pullRequestList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (int id in pullRequestList.OrderBy(rr => rr))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }

                        string result = GetPullRequsetById(id).Result;
                        GitPullRequest pullRequest = JsonSerializer.Deserialize<GitPullRequest>(result);
                        sb.Append($"{pullRequest.PullRequestId}-{pullRequest.Status} ({pullRequest.TargetRefName.Replace("refs/heads/", "")})");
                    }

                    textWriter.Write($",\"{sb}\"");
                }

                textWriter.WriteLine();

                if (childList.Count > 0)
                {
                    PerformWorkItems(childList.ToArray(), textWriter, level + 1);
                }
            }
        }

        private static async Task<string> GetPullRequsetById(int id)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetWorkItemListByIds(IEnumerable<int> workItems)
        {
            var items = JoinToString(workItems, ",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetStringByUri(string uri)
        {
            var pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{PersonalAccessToken}"));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string JoinToString<T>(IEnumerable<T> enumerable, string delimiter)
        {
            IEnumerator<T> enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(enumerator.Current);

            while (enumerator.MoveNext())
            {
                builder.Append(delimiter);
                builder.Append(enumerator.Current);
            }

            return builder.ToString();
        }
    }
}

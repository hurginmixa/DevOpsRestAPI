using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeviceProfileSample
{
    internal static class Program
    {
        private const string PersonalAccessToken = "sdfax7w2hqrnfucryprws7sdjwkjulwvgsbgwuwopfuvi2t4hbbq";
        private const string Organization = "AzCamtek";
        private const string Project = "Falcon";

        static void Main(string[] args)
        {
            var workItems = new int[] {39862, 34598, 39309};

            string responseBody = GetWorkItemListByIds(workItems).Result;
            File.WriteAllText(@"c:\Mixa\Mixa.json", CustJsonSerializer.FormatJson(responseBody));

            WorkItemList workItemList = JsonSerializer.Deserialize<WorkItemList>(responseBody);

            foreach (WorkItem workItem in workItemList.Value)
            {
                foreach (Relation relation in workItem.Relations)
                {
                    if (relation.Attributes.Name == "Child")
                    {
                        string relationUri = relation.Url;

                        string result = CustJsonSerializer.FormatJson(GetSubWorkItemList(relationUri).Result);
                        WorkItem subWorkItem = JsonSerializer.Deserialize<WorkItem>(result);

                        foreach (Relation subRelation in subWorkItem.Relations)
                        {
                            if (subRelation.Attributes.Name == "Pull Request")
                            {
                                string decodedUrl = Uri.UnescapeDataString(subRelation.Url);
                                string txtId = decodedUrl.Substring(decodedUrl.LastIndexOf('/') + 1);

                                string tt = CustJsonSerializer.FormatJson(GetPullRequsetById(int.Parse(txtId)).Result);
                                GitPullRequest pullRequest = JsonSerializer.Deserialize<GitPullRequest>(tt);
                            }
                        }
                    }
                }
            }
        }

        private static async Task<string> GetPullRequsetById(int id)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetWorkItemListByIds(int[] workItems)
        {
            var items = JoinToString(workItems, ",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetSubWorkItemList(string uri)
        {
                uri += "?$expand=relations";

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

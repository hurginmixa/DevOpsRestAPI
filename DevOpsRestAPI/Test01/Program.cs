using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DeviceProfileSample
{
    internal static class Program
    {
        private const string PersonalAccessToken = "";
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

                        string result = GetSubWorkItemList(relationUri).Result;

                        WorkItem subWorkItem = JsonSerializer.Deserialize<WorkItem>(result);
                    }
                }
            }
        }

        private static async Task<string> GetWorkItemListByIds(int[] workItems)
        {
            var pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{PersonalAccessToken}"));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

                var items = JoinToString(workItems, ",");

                var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static async Task<string> GetSubWorkItemList(string uri)
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

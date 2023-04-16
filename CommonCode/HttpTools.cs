using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CommonCode
{
    public static class HttpTools
    {
        private const string Organization = "AzCamtek";
        private const string Project = "Falcon";

        public static async Task<string> GetPullRequestById(int id, string personalAccessToken)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return await GetStringByUri(uri, personalAccessToken);
        }

        public static async Task<string> GetWorkItemListByIds(IEnumerable<int> workItems, string personalAccessToken)
        {
            var items = workItems.JoinToString(",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return await GetStringByUri(uri, personalAccessToken);
        }

        public static async Task<string> GetWorkItemBatch(string json, string personalAccessToken)
        {
            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitemsbatch?api-version=7.0";

            return await PostStringByUri(uri, json, personalAccessToken);
        }

        private static async Task<string> GetStringByUri(string uri, string personalAccessToken)
        {
            var pat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> PostStringByUri(string uri, string json, string personalAccessToken)
        {
            var pat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
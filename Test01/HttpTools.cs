using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Test01
{
    public static class HttpTools
    {
        private const string Organization = "AzCamtek";
        private const string Project = "Falcon";

        public static async Task<string> GetPullRequestById(int id)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return await GetStringByUri(uri);
        }

        public static async Task<string> GetWorkItemListByIds(IEnumerable<int> workItems)
        {
            var items = Tools.JoinToString(workItems, ",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return await GetStringByUri(uri);
        }

        private static async Task<string> GetStringByUri(string uri)
        {
            var pat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{PatContainer.PersonalAccessToken}"));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", pat);

                HttpResponseMessage response = await httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

    }
}
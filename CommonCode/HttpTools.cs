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

        // Один общий HttpClient на всё приложение: он потокобезопасен для запросов
        // и переиспользует TCP-соединения (keep-alive pooling). Так мы избегаем
        // исчерпания сокетов (TIME_WAIT), которое возникает при "new HttpClient()"
        // на каждый вызов.
        private static readonly HttpClient HttpClient = new HttpClient();

        public static Task<string> GetPullRequestById(int id, string personalAccessToken)
        {
            var uri = $"https://dev.azure.com/{Organization}/_apis/git/pullrequests/{id}?api-version=7.1-preview.0";

            return GetStringByUri(uri, personalAccessToken);
        }

        public static Task<string> GetWorkItemListByIds(IEnumerable<int> workItems, string personalAccessToken)
        {
            var items = workItems.JoinToString(",");

            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitems?ids={items}&$expand=all&api-version=7.0";

            return GetStringByUri(uri, personalAccessToken);
        }

        public static Task<string> GetWorkItemBatch(string json, string personalAccessToken)
        {
            var uri = $"https://dev.azure.com/{Organization}/{Project}/_apis/wit/workitemsbatch?api-version=7.0";

            return PostStringByUri(uri, json, personalAccessToken);
        }

        // Заголовок авторизации ставим на каждый HttpRequestMessage отдельно,
        // а не на HttpClient.DefaultRequestHeaders: общий клиент используется
        // из параллельных запросов, и общее изменяемое состояние вызвало бы гонки.
        private static AuthenticationHeaderValue BuildBasicAuth(string personalAccessToken)
        {
            var pat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            return new AuthenticationHeaderValue("Basic", pat);
        }

        private static async Task<string> GetStringByUri(string uri, string personalAccessToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Authorization = BuildBasicAuth(personalAccessToken);

                using HttpResponseMessage response = await HttpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                throw new Exception($"Exception for {uri} arrived", e);
            }
        }

        private static async Task<string> PostStringByUri(string uri, string json, string personalAccessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Authorization = BuildBasicAuth(personalAccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}

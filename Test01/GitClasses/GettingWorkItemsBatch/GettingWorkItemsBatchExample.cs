using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Test01.GitClasses.GettingWorkItemsBatch
{
    public static class GettingWorkItemsBatchExample
    {
        public static async Task GetNewMethod()
        {
            GitWorkItemsBatchRequestBody batchRequestBody = new GitWorkItemsBatchRequestBody
            {
                Ids = new[] {40044, 39309, 36757, 36514, 38280, 31765, 34112, 39244, 39926},

                //Fields = new[] { "System.Id", "System.Title", "System.WorkItemType", "Microsoft.VSTS.Scheduling.RemainingWork" }
            };

            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = CustJsonSerializer.FormatJson(JsonSerializer.Serialize(batchRequestBody, options));

            string result = CustJsonSerializer.FormatJson(await HttpTools.GetWorkItemBatch(json));
            GitWorkItemList gitWorkItemList = JsonSerializer.Deserialize<GitWorkItemList>(result);

            Console.WriteLine($"gitWorkItemList.Count: {gitWorkItemList?.Count ?? -1}");
        }
    }
}

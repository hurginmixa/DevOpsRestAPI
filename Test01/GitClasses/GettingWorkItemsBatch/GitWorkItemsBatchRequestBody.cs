using System.Text.Json.Serialization;

namespace Test01.GitClasses.GettingWorkItemsBatch
{
    public class GitWorkItemsBatchRequestBody
    {
        [JsonPropertyName("fields")]
        public string[] Fields { get; set; } = null;

        [JsonPropertyName("ids")]
        public int[] Ids { get; set; } = null;

        [JsonPropertyName("asOf")]
        public string AsOf { get; set; } = null;

        [JsonPropertyName("errorPolicy")]
        public WorkItemErrorPolicy ErrorPolicy { get; set; } = null;

        [JsonPropertyName("$expand")]
        public WorkItemExpand Expand { get; set; } = null;
    }
}

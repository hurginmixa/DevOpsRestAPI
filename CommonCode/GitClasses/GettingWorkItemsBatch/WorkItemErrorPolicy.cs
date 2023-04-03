using System.Text.Json.Serialization;

namespace CommonCode.GitClasses.GettingWorkItemsBatch
{
    public class WorkItemErrorPolicy
    {
        [JsonPropertyName("fail")]
        public string Fail { get; set; } = null;

        [JsonPropertyName("omit")]
        public string Omit { get; set; } = null;
    }
}
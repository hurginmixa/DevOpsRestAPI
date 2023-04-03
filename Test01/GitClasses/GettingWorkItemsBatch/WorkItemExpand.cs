using System.Text.Json.Serialization;

namespace Test01.GitClasses.GettingWorkItemsBatch
{
    public class WorkItemExpand
    {
        [JsonPropertyName("all")]
        public string All { get; set; } = null;

        [JsonPropertyName("fields")]
        public string Fields { get; set; } = null;

        [JsonPropertyName("links")]
        public string Links { get; set; } = null;

        [JsonPropertyName("none")]
        public string None { get; set; } = null;

        [JsonPropertyName("relations")]
        public string Relations { get; set; } = null;
    }
}
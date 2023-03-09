using System;
using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class WorkItemList
    {
        [JsonPropertyName("count")]
        public int Count { get; set; } = 0;

        [JsonPropertyName("value")]
        public WorkItem[] Value { get; set; } = Array.Empty<WorkItem>();
    }
}
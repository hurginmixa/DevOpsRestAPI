using System;
using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class WorkItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public WorkItemFields Fields { get; set; } = new WorkItemFields();

        [JsonPropertyName("relations")]
        public Relation[] Relations{ get; set; } = Array.Empty<Relation>();
    }
}
using System;
using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class GitWorkItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public GitWorkItemFields Fields { get; set; } = new GitWorkItemFields();

        [JsonPropertyName("relations")]
        public GitRelation[] Relations{ get; set; } = Array.Empty<GitRelation>();
    }
}
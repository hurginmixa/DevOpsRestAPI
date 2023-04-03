using System;
using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
{
    public class GitWorkItem
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("url")] 
        public string Url { get; set; }

        [JsonPropertyName("fields")]
        public GitWorkItemFields Fields { get; set; } = new();

        [JsonPropertyName("relations")] 
        public GitWorkItemRelation[] Relations { get; set; } = Array.Empty<GitWorkItemRelation>();

        [JsonPropertyName("_links")] 
        public GitLinks Links { get; set; }
    }

    public class GitLinks
    {
        [JsonPropertyName("self")] 
        public GitLink Self { get; set; } = new();

        [JsonPropertyName("html")] 
        public GitLink Html{ get; set; } = new();
    }
}
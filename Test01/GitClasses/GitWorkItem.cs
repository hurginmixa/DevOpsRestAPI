using System;
using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitWorkItem
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("fields")]
        public GitWorkItemFields Fields { get; set; } = new();

        [JsonPropertyName("relations")] 
        public GitRelation[] Relations { get; set; } = Array.Empty<GitRelation>();

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
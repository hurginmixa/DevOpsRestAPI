using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
{
    public class GitWorkItemRelation
    {
        [JsonPropertyName("rel")]
        public string Rel { get; set; }
        
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("attributes")]
        public GitRelationAttributes Attributes { get; set; } = new GitRelationAttributes();
    }
}
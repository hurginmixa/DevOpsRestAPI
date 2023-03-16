using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitRepository
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("project")] 
        public GitProject Project { get; set; } = new GitProject();
    }
}
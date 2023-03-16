using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitRelationAttributes
    {
        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("name")] 
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("id")] 
        public int Id { get; set; } = -1;
    }
}
using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class RelationAttributes
    {
        [JsonPropertyName("isLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("name")] 
        public string Name { get; set; } = string.Empty;
    }
}
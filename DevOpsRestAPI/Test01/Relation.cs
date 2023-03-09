using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class Relation
    {
        [JsonPropertyName("rel")]
        public string Rel { get; set; }
        
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("attributes")]
        public RelationAttributes Attributes { get; set; } = new RelationAttributes();
    }
}
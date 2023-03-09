using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class Person
    {
        [JsonPropertyName("displayName")]
        public string DisplayName{ get; set; }
    }
}
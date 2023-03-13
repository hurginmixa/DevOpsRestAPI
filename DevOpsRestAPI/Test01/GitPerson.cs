using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class GitPerson
    {
        [JsonPropertyName("displayName")]
        public string DisplayName{ get; set; }
    }
}
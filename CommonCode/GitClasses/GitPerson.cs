using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
{
    public class GitPerson
    {
        [JsonPropertyName("displayName")]
        public string DisplayName{ get; set; }
    }
}
using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitPerson
    {
        [JsonPropertyName("displayName")]
        public string DisplayName{ get; set; }
    }
}
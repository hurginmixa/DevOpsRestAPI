using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
{
    public class GitLink
    {
        [JsonPropertyName("href")] 
        public string Href { get; set; } = string.Empty;
    }
}
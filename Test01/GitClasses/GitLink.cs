using System.Text.Json.Serialization;

namespace Test01.GitClasses
{
    public class GitLink
    {
        [JsonPropertyName("href")] 
        public string Href { get; set; } = string.Empty;
    }
}
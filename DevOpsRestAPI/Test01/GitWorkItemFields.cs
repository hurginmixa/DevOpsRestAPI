using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class GitWorkItemFields
    {
        [JsonPropertyName("System.Id")]
        public int Id { get; set; }

        [JsonPropertyName("System.WorkItemType")]
        public string WorkItemType { get; set; }

        [JsonPropertyName("System.Title")]
        public string Title { get; set; }

        [JsonPropertyName("System.State")]
        public string State { get; set; }
        
        [JsonPropertyName("System.AssignedTo")]
        public GitPerson AssignedTo { get; set; }
        
        [JsonPropertyName("System.CreatedBy")]
        public GitPerson CreatedBy { get; set; }
    }
}
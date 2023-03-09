using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class WorkItemFields
    {
        [JsonPropertyName("System.Id")]
        public int Id { get; set; }

        [JsonPropertyName("System.WorkItemType")]
        public string WorkItemType{ get; set; }

        [JsonPropertyName("System.State")]
        public string State{ get; set; }
        
        [JsonPropertyName("System.AssignedTo")]
        public Person AssignedTo{ get; set; }
        
        [JsonPropertyName("System.CreatedBy")]
        public Person CreatedBy{ get; set; }
    }
}
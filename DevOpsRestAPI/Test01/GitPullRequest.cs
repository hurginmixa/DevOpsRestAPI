using System;
using System.Text.Json.Serialization;

namespace DeviceProfileSample
{
    public class GitPullRequest
    {
        [JsonPropertyName("pullRequestId")]
        public int Id { get; set; }

        [JsonPropertyName("codeReviewId")]
        public int CodeReviewId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("sourceRefName")]
        public string SourceRefName { get; set; }

        [JsonPropertyName("targetRefName")]
        public string TargetRefName { get; set; }

        [JsonPropertyName("mergeStatus")]
        public string MergeStatus { get; set; }

        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("closedDate")]
        public DateTime ClosedDate { get; set; }

        [JsonPropertyName("repository")] 
        public GitRepository Repository { get; set; } = new GitRepository();
    }
}

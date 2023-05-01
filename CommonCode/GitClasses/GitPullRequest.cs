using System;
using System.Text.Json.Serialization;

namespace CommonCode.GitClasses
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

        [JsonPropertyName("createdBy")] 
        public GitPullRequestPeople CreatedBy { get; set; } = new GitPullRequestPeople();

        [JsonPropertyName("closedBy")] 
        public GitPullRequestPeople ClosedBy { get; set; } = new GitPullRequestPeople();

        [JsonPropertyName("lastMergeSourceCommit")]
        public GitPullRequestCommitInfo LastMergeSourceCommit { get; set; } = new GitPullRequestCommitInfo();

        [JsonPropertyName("lastMergeTargetCommit")]
        public GitPullRequestCommitInfo LastMergeTargetCommit { get; set; } = new GitPullRequestCommitInfo();

        [JsonPropertyName("lastMergeCommit")]
        public GitPullRequestCommitInfo LastMergeCommit { get; set; } = new GitPullRequestCommitInfo();
    }

    public class GitPullRequestPeople
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }

    public class GitPullRequestCommitInfo
    {
        [JsonPropertyName("commitId")]
        public string CommitId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}

using System;

namespace CommonCode.DocumentClasses.SerializeClasses
{
    public class DocumentWorkItemData
    {
        public int Id { get; set; }

        public string WorkItemType { get; set; }

        public string State { get; set; }

        public string Title { get; set; }

        public string Html { get; set; }

        public DocumentWorkItemData[] SubItemList { get; set; } = Array.Empty<DocumentWorkItemData>();

        public DocumentPullRequestData[] PullRequestList { get; set; } = Array.Empty<DocumentPullRequestData>();
    }
}

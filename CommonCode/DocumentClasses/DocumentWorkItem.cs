using System.Collections.Generic;
using System.Linq;
using CommonCode.GitClasses;

namespace CommonCode.DocumentClasses
{
    public class DocumentWorkItem : IDocumentWorkItem
    {
        private readonly int _id;
        private readonly string _workItemType;
        private readonly string _state;
        private readonly string _title;
        private readonly string _html;

        private readonly DocumentWorkItemList _workItemList = new();
        private readonly List<DocumentPullRequest> _pullRequestList = new();

        public DocumentWorkItem(GitWorkItem workItem)
        {
            _id = workItem.Id;
            _workItemType = workItem.Fields.WorkItemType;
            _state = workItem.Fields.State;
            _title = workItem.Fields.Title;
            _html = workItem.Links.Html.Href;
        }

        public IDocumentWorkItemList SubItems => _workItemList;

        public IEnumerable<DocumentPullRequest> PullRequestList => _pullRequestList;

        public IEnumerable<DocumentPullRequest> GetFullPullRequestList()
        {
            HashSet<DocumentPullRequest> pullRequests = new HashSet<DocumentPullRequest>();

            foreach (DocumentPullRequest pullRequest in PullRequestList)
            {
                pullRequests.Add(pullRequest);
            }

            foreach (var subItem in SubItems.GetWorkItems())
            {
                foreach (DocumentPullRequest pullRequest in subItem.GetFullPullRequestList())
                {
                    pullRequests.Add(pullRequest);
                }
            }

            return pullRequests;
        }

        public bool HasActiveSubItems
        {
            get
            {
                if (IsActive || IsInProgress || IsInvestigation || IsProposed || IsInProgress)
                {
                    return true;
                }

                return _workItemList.GetWorkItems().Any(r => r.HasActiveSubItems);
            }
        }

        public void AddPullRequest(DocumentPullRequest pullRequest)
        {
            _pullRequestList.Add(pullRequest);
        }

        public int Id => _id;

        public string WorkItemType => _workItemType;

        public string State => _state;

        public string Title => _title;
        
        public bool IsClosed => _state == "Closed";

        public bool IsInProgress => _state == "In Progress";
        
        public bool IsActive => _state == "Active";
        
        public bool IsProposed => _state == "Proposed";
        
        public bool IsInvestigation => _state == "Investigation";

        public bool IsTaskValidation => _state == "Task-Validation";

        public string Html => _html;

        private bool Equals(DocumentWorkItem other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DocumentWorkItem) obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}
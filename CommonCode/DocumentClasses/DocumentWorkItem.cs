using System.Collections.Generic;
using System.Linq;
using CommonCode.DocumentClasses.SerializeClasses;
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

        private readonly DocumentWorkItemList _subItemList = new();
        private readonly List<DocumentPullRequest> _pullRequestList = new();

        public DocumentWorkItem(GitWorkItem workItem)
        {
            _id = workItem.Id;
            _workItemType = workItem.Fields.WorkItemType;
            _state = workItem.Fields.State;
            _title = workItem.Fields.Title;
            _html = workItem.Links.Html.Href;
        }

        public DocumentWorkItemData GetData()
        {
            DocumentWorkItemData workItemData = new DocumentWorkItemData();

            workItemData.Id = _id;
            workItemData.WorkItemType = _workItemType;
            workItemData.State = _state;
            workItemData.Title = _title;
            workItemData.Html = _html;

            workItemData.SubItemList = _subItemList.GetData();

            workItemData.PullRequestList = _pullRequestList.Select(k => k.GetData()).ToArray();

            return workItemData;
        }

        public IDocumentWorkItemList SubItems => _subItemList;

        public IEnumerable<DocumentPullRequest> PullRequestList => _pullRequestList;

        public IEnumerable<(DocumentPullRequest Request, bool IsOwner)> GetFullPullRequestList()
        {
            HashSet<(DocumentPullRequest Request, bool IsOwner)> pullRequests = new();

            foreach (DocumentPullRequest pullRequest in PullRequestList)
            {
                pullRequests.Add((Request: pullRequest, IsOwner: true));
            }

            foreach (var subItem in SubItems)
            {
                foreach ((DocumentPullRequest Request, bool IsOwner) pullRequest in subItem.GetFullPullRequestList())
                {
                    pullRequests.Add((Request: pullRequest.Request, IsOwner: false));
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

                return _subItemList.Any(r => r.HasActiveSubItems);
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
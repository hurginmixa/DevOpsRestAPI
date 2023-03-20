﻿using System.Collections.Generic;
using Test01.GitClasses;

namespace Test01.DocumentClasses
{
    public class DocumentWorkItem
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

            foreach (var pullRequest in PullRequestList)
            {
                pullRequests.Add(pullRequest);
            }

            foreach (var subItem in SubItems.GetWorkItems())
            {
                foreach (var pullRequest in subItem.GetFullPullRequestList())
                {
                    pullRequests.Add(pullRequest);
                }
            }

            return pullRequests;
        }

        public void AddPullRequest(DocumentPullRequest pullRequest)
        {
            _pullRequestList.Add(pullRequest);
        }

        public int Id => _id;

        public string WorkItemType => _workItemType;

        public string State => _state;

        public string Title => _title;

        public string Html => _html;
    }
}
using System.Collections.Generic;
using CommonCode.DocumentClasses.SerializeClasses;

namespace CommonCode.DocumentClasses
{
    public interface IDocumentWorkItem
    {
        int Id { get; }

        IDocumentWorkItemList SubItems { get; }
        
        IEnumerable<(DocumentPullRequest Request, bool IsOwner)> GetFullPullRequestList();

        bool HasActiveSubItems { get; }

        IEnumerable<DocumentPullRequest> PullRequestList { get; }

        DocumentWorkItemData GetData();
        
        string Html { get; }
        
        string State { get; }

        string AssignedTo { get; }
        
        string WorkItemType { get; }
        
        string Title { get; }

        bool IsClosed { get; }
        
        bool IsResolved { get; }
        
        bool IsInProgress { get; }
        
        bool IsActive { get; }
        
        bool IsProposed { get; }
        
        bool IsInvestigation { get; }

        bool IsTaskValidation { get; }
    }
}
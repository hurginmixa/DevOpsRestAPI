using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public interface IDocumentWorkItem
    {
        int Id { get; }

        IDocumentWorkItemList SubItems { get; }
        
        IEnumerable<DocumentPullRequest> GetFullPullRequestList();

        bool HasActiveSubItems { get; }
        
        string Html { get; }
        
        string State { get; }
        
        string WorkItemType { get; }
        
        string Title { get; }

        bool IsClosed { get; }
        
        bool IsInProgress { get; }
        
        bool IsActive { get; }
        
        bool IsProposed { get; }
        
        bool IsInvestigation { get; }

        bool IsTaskValidation { get; }
    }
}
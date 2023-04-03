using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public interface IDocumentWorkItemList
    {
        void AddWorkItem(DocumentWorkItem item);

        IEnumerable<DocumentWorkItem> GetWorkItems();
        
        void RemoveItem(DocumentWorkItem workItem);
    }
}
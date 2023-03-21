using System.Collections.Generic;

namespace Test01.DocumentClasses
{
    public interface IDocumentWorkItemList
    {
        void AddWorkItem(DocumentWorkItem item);

        IEnumerable<DocumentWorkItem> GetWorkItems();
        
        void RemoveItem(DocumentWorkItem workItem);
    }
}
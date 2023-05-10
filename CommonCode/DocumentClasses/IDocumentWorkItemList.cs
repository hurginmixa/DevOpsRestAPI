using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public interface IDocumentWorkItemList
    {
        void AddWorkItem(IDocumentWorkItem item);

        IEnumerable<IDocumentWorkItem> GetWorkItems();
        
        void RemoveItem(IDocumentWorkItem workItem);
    }
}
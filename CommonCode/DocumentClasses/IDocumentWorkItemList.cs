using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public interface IDocumentWorkItemList : IEnumerable<IDocumentWorkItem>
    {
        void AddWorkItem(IDocumentWorkItem item);
        
        void RemoveItem(IDocumentWorkItem workItem);
    }
}
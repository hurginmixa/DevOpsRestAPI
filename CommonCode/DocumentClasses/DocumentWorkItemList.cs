using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public class DocumentWorkItemList : IDocumentWorkItemList
    {
        private readonly object _sync = new object();
        private readonly List<IDocumentWorkItem> _list = new();

        public DocumentWorkItemList()
        {
        }

        public DocumentWorkItemList(IEnumerable<IDocumentWorkItem> list) : this()
        {
            _list.AddRange(list);
        }

        public void AddWorkItem(IDocumentWorkItem item)
        {
            lock (_sync)
            {
                _list.Add(item);
            }
        }

        public IEnumerable<IDocumentWorkItem> GetWorkItems() => _list;

        public void RemoveItem(IDocumentWorkItem workItem) => _list.Remove(workItem);
    }
}
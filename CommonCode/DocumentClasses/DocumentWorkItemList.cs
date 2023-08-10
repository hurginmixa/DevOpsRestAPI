using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommonCode.DocumentClasses.SerializeClasses;

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

        public DocumentWorkItemData[] GetData() => _list.Select(i => i.GetData()).ToArray();

        public void AddWorkItem(IDocumentWorkItem item)
        {
            lock (_sync)
            {
                _list.Add(item);
            }
        }

        public void RemoveItem(IDocumentWorkItem workItem) => _list.Remove(workItem);

        public IEnumerator<IDocumentWorkItem> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
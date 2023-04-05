﻿using System.Collections.Generic;

namespace CommonCode.DocumentClasses
{
    public class DocumentWorkItemList : IDocumentWorkItemList
    {
        private readonly object _sync = new object();
        private readonly List<DocumentWorkItem> _list = new();

        public void AddWorkItem(DocumentWorkItem item)
        {
            lock (_sync)
            {
                _list.Add(item);
            }
        }

        public IEnumerable<DocumentWorkItem> GetWorkItems() => _list;

        public void RemoveItem(DocumentWorkItem workItem) => _list.Remove(workItem);
    }
}
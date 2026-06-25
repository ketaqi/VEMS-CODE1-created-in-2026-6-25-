using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RoslynPad.UI
{
    internal sealed class DocumentCollection : ObservableCollection<DocumentViewModel>
    {
        // 使用 OrdinalIgnoreCase，满足 CA1309，且键查找稳定高效
        private readonly Dictionary<string, DocumentViewModel> _dictionary;

        public DocumentCollection(IEnumerable<DocumentViewModel> items)
        {
            _dictionary = new Dictionary<string, DocumentViewModel>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                Add(item);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _dictionary.Clear();
        }

        protected override void InsertItem(int index, DocumentViewModel item)
        {
            // 先查重：存在同名则替换
            if (_dictionary.TryGetValue(item.Name, out var existing))
            {
                var oldIndex = Items.IndexOf(existing);
                if (oldIndex >= 0)
                {
                    base.SetItem(oldIndex, item);
                    _dictionary[item.Name] = item;
                    return;
                }
                // 理论上不会走到这里；兜底按新项插入
            }

            base.InsertItem(index, item);
            _dictionary.Add(item.Name, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            base.RemoveItem(index);
            _dictionary.Remove(item.Name);
        }

        protected override void SetItem(int index, DocumentViewModel item)
        {
            var old = Items[index];
            base.SetItem(index, item);

            // 用 OrdinalIgnoreCase 比较名称，满足 CA1309
            if (!string.Equals(old.Name, item.Name, StringComparison.OrdinalIgnoreCase))
            {
                _dictionary.Remove(old.Name);
            }
            _dictionary[item.Name] = item;
        }

        public DocumentViewModel? this[string name]
        {
            get
            {
                _dictionary.TryGetValue(name, out var item);
                return item;
            }
        }
    }
}

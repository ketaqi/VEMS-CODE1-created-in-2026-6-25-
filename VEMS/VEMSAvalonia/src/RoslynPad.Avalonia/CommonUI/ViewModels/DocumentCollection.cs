using System.Collections.ObjectModel;

namespace RoslynPad.UI;

/// <summary>
/// 文档集合：支持按名称快速查找的可观察文档集合。
/// </summary>
/// <remarks>
/// <para>
/// 此类扩展 <see cref="ObservableCollection{T}"/>，添加了按文档名称索引的能力。
/// 内部维护一个字典以提供 O(1) 时间复杂度的名称查找。
/// </para>
/// <para>
/// 设计特点：
/// <list type="bullet">
///   <item><description>名称比较使用 <see cref="StringComparer.OrdinalIgnoreCase"/>，不区分大小写</description></item>
///   <item><description>插入同名文档时自动替换现有项</description></item>
///   <item><description>所有集合操作自动同步内部字典</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var documents = new DocumentCollection(existingDocs);
/// 
/// // 按名称查找
/// var readme = documents["README.md"];
/// 
/// // 添加新文档（如果同名则替换）
/// documents.Add(newDocument);
/// 
/// // 遍历所有文档
/// foreach (var doc in documents)
/// {
///     Console.WriteLine(doc.Name);
/// }
/// </code>
/// </example>
internal sealed class DocumentCollection : ObservableCollection<DocumentViewModel>
{
    /// <summary>
    /// 名称到文档的映射字典，使用不区分大小写的比较器。
    /// </summary>
    private readonly Dictionary<string, DocumentViewModel> _dictionary;

    /// <summary>
    /// 使用指定的文档集合初始化 <see cref="DocumentCollection"/> 类的新实例。
    /// </summary>
    /// <param name="items">要添加到集合的初始文档。</param>
    public DocumentCollection(IEnumerable<DocumentViewModel> items)
    {
        _dictionary = new Dictionary<string, DocumentViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// 清除集合中的所有项。
    /// </summary>
    /// <remarks>
    /// 同时清空内部的名称索引字典。
    /// </remarks>
    protected override void ClearItems()
    {
        base.ClearItems();
        _dictionary.Clear();
    }

    /// <summary>
    /// 在指定位置插入文档。
    /// </summary>
    /// <param name="index">要插入的位置索引。</param>
    /// <param name="item">要插入的文档。</param>
    /// <remarks>
    /// 如果已存在同名文档，将替换现有项而非插入新项。
    /// </remarks>
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

    /// <summary>
    /// 移除指定位置的文档。
    /// </summary>
    /// <param name="index">要移除的文档的位置索引。</param>
    protected override void RemoveItem(int index)
    {
        var item = Items[index];
        base.RemoveItem(index);
        _dictionary.Remove(item.Name);
    }

    /// <summary>
    /// 替换指定位置的文档。
    /// </summary>
    /// <param name="index">要替换的位置索引。</param>
    /// <param name="item">新的文档。</param>
    /// <remarks>
    /// 如果新文档的名称与被替换文档不同，将更新名称索引。
    /// </remarks>
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

    /// <summary>
    /// 通过文档名称获取文档。
    /// </summary>
    /// <param name="name">文档名称（不区分大小写）。</param>
    /// <returns>
    /// 如果找到匹配的文档，返回该文档；否则返回 <c>null</c>。
    /// </returns>
    /// <example>
    /// <code>
    /// var doc = collection["Program.cs"];
    /// if (doc != null)
    /// {
    ///     // 处理文档
    /// }
    /// </code>
    /// </example>
    public DocumentViewModel? this[string name]
    {
        get
        {
            _dictionary.TryGetValue(name, out var item);
            return item;
        }
    }
}

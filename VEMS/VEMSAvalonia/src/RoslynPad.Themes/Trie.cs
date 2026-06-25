namespace RoslynPad.Themes;

/// <summary>
/// 泛型前缀树（Trie）实现，用于高效的前缀匹配查找。
/// </summary>
/// <typeparam name="T">存储值的类型。</typeparam>
internal class Trie<T>
{
    private const char Separator = '.';
    private readonly TrieNode _root = new();

    /// <summary>
    /// 尝试向前缀树中添加键值对。如果键已存在，则不会覆盖。
    /// </summary>
    /// <param name="key">以点号分隔的键（如 "entity.name.function"）。</param>
    /// <param name="value">要存储的值。</param>
    public void TryAdd(string key, T value)
    {
        var node = _root;
        var parts = key.Split(Separator);
        foreach (var part in parts)
        {
            if (!node.Children.TryGetValue(part, out var childNode))
            {
                childNode = new TrieNode();
                node.Children.Add(part, childNode);
            }

            node = childNode;
        }

        node.Value ??= new KeyValuePair<string, T>(key, value);
    }

    /// <summary>
    /// 查找与给定键匹配的最长前缀对应的值。
    /// </summary>
    /// <param name="key">要查找的键。</param>
    /// <returns>匹配的键值对，如果未找到则返回 <c>null</c>。</returns>
    public KeyValuePair<string, T>? FindLongestPrefix(string key)
    {
        var node = _root;
        var parts = key.Split(Separator);
        foreach (var part in parts)
        {
            if (!node.Children.TryGetValue(part, out var childNode))
            {
                break;
            }

            node = childNode;
        }

        return node.Value;
    }

    /// <summary>
    /// 前缀树节点。
    /// </summary>
    private class TrieNode
    {
        /// <summary>
        /// 获取子节点字典。
        /// </summary>
        public Dictionary<string, TrieNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取或设置节点存储的值。
        /// </summary>
        public KeyValuePair<string, T>? Value { get; set; }
    }
}

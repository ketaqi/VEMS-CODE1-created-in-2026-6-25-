// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
using System.Text.RegularExpressions;

namespace RoslynPad.Editor;

/// <summary>
/// 代码片段类，用于处理代码片段的解析、转换及AvalonEdit适配
/// </summary>
/// <param name="name">代码片段名称</param>
/// <param name="description">代码片段描述</param>
/// <param name="text">代码片段原始文本</param>
/// <param name="keyword">代码片段触发关键字</param>
internal sealed partial class CodeSnippet(string name, string description, string text, string keyword)
{
    /// <summary>
    /// 获取代码片段名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 获取代码片段原始文本
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// 获取代码片段描述信息
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// 获取一个值，指示代码片段是否包含${Selection}占位符
    /// </summary>
    public bool HasSelection
    {
        get
        {
            return s_pattern.Matches(Text)
                .OfType<Match>()
                .Any(item => item.Value == "${Selection}");
        }
    }

    /// <summary>
    /// 获取代码片段触发关键字
    /// </summary>
    public string Keyword { get; } = keyword;

    /// <summary>
    /// 创建适用于AvalonEdit的代码片段对象
    /// </summary>
    /// <returns>AvalonEdit兼容的<Snippet>实例</returns>
    public Snippet CreateAvalonEditSnippet()
    {
        return CreateAvalonEditSnippet(Text);
    }

    /// <summary>
    /// 匹配代码片段占位符的正则表达式（自动生成）
    /// </summary>
    private static readonly Regex s_pattern = Pattern();

    /// <summary>
    /// 将指定的代码片段文本转换为AvalonEdit兼容的<Snippet>对象
    /// </summary>
    /// <param name="snippetText">代码片段原始文本</param>
    /// <returns>AvalonEdit兼容的<Snippet>实例</returns>
    /// <exception cref="ArgumentNullException">当<paramref name="snippetText"/>为null时抛出</exception>
    public static Snippet CreateAvalonEditSnippet(string snippetText)
    {
        ArgumentNullException.ThrowIfNull(snippetText);
        var replaceableElements = new Dictionary<string, SnippetReplaceableTextElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var match in s_pattern.Matches(snippetText).OfType<Match>())
        {
            var val = match.Groups[1].Value;
            var equalsSign = val.IndexOf('=');
            if (equalsSign > 0)
            {
                var name = val.Substring(0, equalsSign);
                replaceableElements[name] = new SnippetReplaceableTextElement();
            }
        }
        var snippet = new Snippet();
        var pos = 0;
        foreach (var match in s_pattern.Matches(snippetText).OfType<Match>())
        {
            if (pos < match.Index)
            {
                snippet.Elements.Add(new SnippetTextElement { Text = snippetText.Substring(pos, match.Index - pos) });
            }
            snippet.Elements.Add(CreateElementForValue(replaceableElements, match.Groups[1].Value, match.Index, snippetText));
            pos = match.Index + match.Length;
        }
        if (pos < snippetText.Length)
        {
            snippet.Elements.Add(new SnippetTextElement { Text = snippetText.Substring(pos) });
        }
        if (!snippet.Elements.Any(e => e is SnippetCaretElement))
        {
            var element = snippet.Elements.Select((s, i) => new { s, i }).FirstOrDefault(s => s.s is SnippetSelectionElement);
            var index = element?.i ?? -1;
            if (index > -1)
                snippet.Elements.Insert(index + 1, new SnippetCaretElement());
        }
        return snippet;
    }

    /// <summary>
    /// 匹配代码片段函数占位符的正则表达式（自动生成）
    /// </summary>
    private static readonly Regex s_functionPattern = FunctionPattern();

    /// <summary>
    /// 根据占位符值创建对应的AvalonEdit代码片段元素
    /// </summary>
    /// <param name="replaceableElements">可替换文本元素字典</param>
    /// <param name="val">占位符内的原始值</param>
    /// <param name="offset">占位符在文本中的偏移量</param>
    /// <param name="snippetText">完整的代码片段文本</param>
    /// <returns>对应的<SnippetElement>实例</returns>
    private static SnippetElement CreateElementForValue(Dictionary<string, SnippetReplaceableTextElement> replaceableElements, string val, int offset, string snippetText)
    {
        SnippetReplaceableTextElement? srte;
        var equalsSign = val.IndexOf('=');
        if (equalsSign > 0)
        {
            var name = val.Substring(0, equalsSign);
            if (replaceableElements.TryGetValue(name, out srte))
            {
                srte.Text ??= val.Substring(equalsSign + 1);
                return srte;
            }
        }

        var element = GetDefaultElement(val, snippetText, offset);
        if (element != null)
            return element;

        if (replaceableElements.TryGetValue(val, out srte))
            return new SnippetBoundElement { TargetElement = srte };
        var m = s_functionPattern.Match(val);
        if (m.Success)
        {
            var f = GetFunction(m.Groups[1].Value);
            if (f != null)
            {
                var innerVal = m.Groups[2].Value;
                if (replaceableElements.TryGetValue(innerVal, out srte))
                    return new FunctionBoundElement { TargetElement = srte, _function = f };
                var result2 = GetValue(innerVal);
                if (result2 != null)
                    return new SnippetTextElement { Text = f(result2) };
                return new SnippetTextElement { Text = f(innerVal) };
            }
        }
        var result = GetValue(val);
        if (result != null)
            return new SnippetTextElement { Text = result };
        return new SnippetReplaceableTextElement { Text = val }; // ${unknown} -> replaceable element
    }

    /// <summary>
    /// 获取指定属性名称对应的取值（如类名）
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性对应的取值，无匹配时返回null</returns>
    private static string? GetValue(string propertyName)
    {
        if ("ClassName".Equals(propertyName, StringComparison.OrdinalIgnoreCase))
        {
            var c = GetCurrentClass();
            if (c != null)
            {
                return c;
            }
        }
        //return Core.StringParser.GetValue(propertyName);
        return null;
    }

    /// <summary>
    /// 获取内置占位符（如Selection/Caret）对应的默认元素
    /// </summary>
    /// <param name="tag">占位符标签</param>
    /// <param name="snippetText">完整的代码片段文本</param>
    /// <param name="position">占位符在文本中的位置</param>
    /// <returns>对应的<SnippetElement>实例，无匹配时返回null</returns>
    private static SnippetElement? GetDefaultElement(string tag, string snippetText, int position)
    {
        if ("Selection".Equals(tag, StringComparison.OrdinalIgnoreCase))
        {
            return new SnippetSelectionElement { Indentation = GetWhitespaceBefore(snippetText, position).Length };
        }

        if ("Caret".Equals(tag, StringComparison.OrdinalIgnoreCase))
        {
            // 如果存在${Selection}，仅当有文本选中时才使用${Caret}
            // （无文本选中时，${Selection}会自动设置光标位置）
            return snippetText.Contains("${Selection}", StringComparison.OrdinalIgnoreCase)
                ? new SnippetCaretElement(setCaretOnlyIfTextIsSelected: true)
                : new SnippetCaretElement();
        }

        return null;
    }

    /// <summary>
    /// 获取指定位置之前的空白字符
    /// </summary>
    /// <param name="snippetText">完整的代码片段文本</param>
    /// <param name="offset">目标位置偏移量</param>
    /// <returns>空白字符子串</returns>
    private static string GetWhitespaceBefore(string snippetText, int offset)
    {
        var start = snippetText.LastIndexOfAny(['\r', '\n'], offset) + 1;
        return snippetText.Substring(start, offset - start);
    }

    /// <summary>
    /// 获取当前上下文的类名（待实现）
    /// </summary>
    /// <returns>当前类名，未实现故返回null</returns>
    private static string? GetCurrentClass()
    {
        // TODO
        return null;
    }

    /// <summary>
    /// 根据函数名获取对应的字符串处理函数
    /// </summary>
    /// <param name="name">函数名称（如toLower/toUpper）</param>
    /// <returns>对应的字符串处理委托，无匹配时返回null</returns>
    private static Func<string, string>? GetFunction(string name)
    {
        if ("toLower".Equals(name, StringComparison.OrdinalIgnoreCase))
            return s => s.ToLowerInvariant();
        if ("toUpper".Equals(name, StringComparison.OrdinalIgnoreCase))
            return s => s.ToUpperInvariant();
        if ("toFieldName".Equals(name, StringComparison.OrdinalIgnoreCase))
            return GetFieldName;
        if ("toPropertyName".Equals(name, StringComparison.OrdinalIgnoreCase))
            return GetPropertyName;
        if ("toParameterName".Equals(name, StringComparison.OrdinalIgnoreCase))
            return GetParameterName;
        return null;
    }

    /// <summary>
    /// 将字段名转换为属性名（首字母大写，移除前缀下划线/m_）
    /// </summary>
    /// <param name="fieldName">原始字段名</param>
    /// <returns>转换后的属性名</returns>
    private static string GetPropertyName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;
        if (fieldName.StartsWith('_') && fieldName.Length > 1)
            return char.ToUpperInvariant(fieldName[1]) + fieldName.Substring(2);
        if (fieldName.StartsWith("m_", StringComparison.Ordinal) && fieldName.Length > 2)
            return char.ToUpperInvariant(fieldName[2]) + fieldName.Substring(3);
        return char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
    }

    /// <summary>
    /// 将字段名转换为参数名（首字母小写，移除前缀下划线/m_）
    /// </summary>
    /// <param name="fieldName">原始字段名</param>
    /// <returns>转换后的参数名</returns>
    private static string GetParameterName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return fieldName;
        if (fieldName.StartsWith('_') && fieldName.Length > 1)
            return char.ToLowerInvariant(fieldName[1]) + fieldName.Substring(2);
        if (fieldName.StartsWith("m_", StringComparison.Ordinal) && fieldName.Length > 2)
            return char.ToLowerInvariant(fieldName[2]) + fieldName.Substring(3);
        return char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
    }

    /// <summary>
    /// 将属性名转换为字段名（首字母小写，必要时添加前缀下划线）
    /// </summary>
    /// <param name="propertyName">原始属性名</param>
    /// <returns>转换后的字段名</returns>
    private static string GetFieldName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;
        var newName = char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
        if (newName == propertyName)
            return "_" + newName;
        return newName;
    }

    /// <summary>
    /// 带函数绑定的代码片段元素类
    /// </summary>
    private sealed class FunctionBoundElement : SnippetBoundElement
    {
        /// <summary>
        /// 字符串处理函数委托
        /// </summary>
        internal Func<string, string>? _function;

        /// <summary>
        /// 将输入文本通过绑定函数转换后返回
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <returns>转换后的文本</returns>
        public override string? ConvertText(string input)
        {
            return _function?.Invoke(input);
        }
    }

    /// <summary>
    /// 生成匹配${...}占位符的正则表达式
    /// </summary>
    /// <returns>编译后的正则表达式实例</returns>
    [GeneratedRegex(@"\$\{([^\}]*)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();

    /// <summary>
    /// 生成匹配函数型占位符（如toLower(xxx)）的正则表达式
    /// </summary>
    /// <returns>编译后的正则表达式实例</returns>
    [GeneratedRegex(@"^([a-zA-Z]+)\(([^\)]*)\)$", RegexOptions.CultureInvariant)]
    private static partial Regex FunctionPattern();
}

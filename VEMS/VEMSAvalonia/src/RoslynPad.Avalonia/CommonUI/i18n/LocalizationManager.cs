using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

namespace RoslynPad;

/// <summary>
/// 本地化资源管理器：负责加载和管理多语言资源字符串。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现单例模式，通过 <see cref="Instance"/> 属性获取全局唯一实例。
/// 支持从 XML 格式的语言文件中加载本地化字符串，并通过索引器提供访问。
/// </para>
/// <para>
/// 语言文件格式要求：
/// <list type="bullet">
///   <item><description>文件位置：<c>i18n/MainModule.{culture}.xml</c>（如 <c>MainModule.zh-CN.xml</c>）</description></item>
///   <item><description>XML 结构：根元素下按模块分组，子元素为具体的资源键值对</description></item>
/// </list>
/// </para>
/// <para>
/// 与 XAML 绑定配合使用：
/// <code>
/// &lt;TextBlock Text="{Binding Localized[MainWindow.MenuItem1]}" /&gt;
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 切换语言
/// LocalizationManager.Instance.LoadLanguage(new CultureInfo("zh-CN"));
/// 
/// // 获取本地化字符串
/// string menuText = LocalizationManager.Instance["MainWindow.MenuItem1"];
/// </code>
/// </example>
public class LocalizationManager : INotifyPropertyChanged
{
    /// <summary>
    /// 获取 <see cref="LocalizationManager"/> 的全局单例实例。
    /// </summary>
    /// <value>
    /// 本地化管理器的唯一实例，在应用程序生命周期内保持不变。
    /// </value>
    public static LocalizationManager Instance { get; } = new LocalizationManager();

    /// <summary>
    /// 存储当前语言的所有本地化资源键值对。
    /// </summary>
    /// <remarks>
    /// 键格式为 "ModuleName.ResourceKey"，如 "MainWindow.MenuItem1"。
    /// </remarks>
    private Dictionary<string, string> _resources = new();

    /// <summary>
    /// 当资源集合发生变化时触发，用于通知 UI 更新绑定。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 通过资源键获取对应的本地化字符串。
    /// </summary>
    /// <param name="key">资源键，格式为 "ModuleName.ResourceKey"。</param>
    /// <returns>
    /// 如果找到对应的本地化字符串则返回该字符串；
    /// 否则返回原始键名（便于调试时识别缺失的资源）。
    /// </returns>
    /// <example>
    /// <code>
    /// // 获取菜单项文本
    /// string text = LocalizationManager.Instance["MainWindow.MenuItem1"];
    /// 
    /// // 在 XAML 中绑定
    /// // Text="{Binding Localized[MainWindow.MenuItem1]}"
    /// </code>
    /// </example>
    public string this[string key] => _resources.TryGetValue(key, out var value) ? value : key;

    /// <summary>
    /// 加载指定区域性的语言资源文件。
    /// </summary>
    /// <param name="culture">
    /// 要加载的区域性信息，如 <c>new CultureInfo("zh-CN")</c> 或 <c>new CultureInfo("en-US")</c>。
    /// </param>
    /// <remarks>
    /// <para>
    /// 此方法会清空当前已加载的资源，然后从对应的 XML 文件中重新加载。
    /// 加载完成后会触发 <see cref="PropertyChanged"/> 事件通知所有绑定更新。
    /// </para>
    /// <para>
    /// 语言文件路径格式：<c>i18n/MainModule.{culture.Name}.xml</c>
    /// </para>
    /// <para>
    /// XML 文件结构示例：
    /// <code>
    /// &lt;Localization&gt;
    ///   &lt;MainWindow&gt;
    ///     &lt;MenuItem1&gt;文件&lt;/MenuItem1&gt;
    ///     &lt;MenuItem2&gt;编辑&lt;/MenuItem2&gt;
    ///   &lt;/MainWindow&gt;
    /// &lt;/Localization&gt;
    /// </code>
    /// 解析后的键为 "MainWindow.MenuItem1"、"MainWindow.MenuItem2"。
    /// </para>
    /// </remarks>
    /// <exception cref="System.IO.FileNotFoundException">
    /// 当指定的语言文件不存在时抛出。
    /// </exception>
    /// <exception cref="System.Xml.XmlException">
    /// 当语言文件格式无效时抛出。
    /// </exception>
    /// <example>
    /// <code>
    /// // 切换到中文
    /// LocalizationManager.Instance.LoadLanguage(new CultureInfo("zh-CN"));
    /// 
    /// // 切换到英文
    /// LocalizationManager.Instance.LoadLanguage(new CultureInfo("en-US"));
    /// </code>
    /// </example>
    public void LoadLanguage(CultureInfo culture)
    {
        var filePath = $"i18n/MainModule.{culture.Name}.xml";
        Console.WriteLine("加载语言文件路径: " + filePath);

        var xdoc = XDocument.Load(filePath);

        _resources.Clear();
        int count = 0;
        foreach (var element in xdoc.Descendants())
        {
            if (element.Parent?.Name != "Localization")
            {
                var key = $"{element.Parent?.Name}.{element.Name}";
                Console.WriteLine($"加载资源: {key} = {element.Value}");
                _resources[key] = element.Value;
                count++;
            }
        }
        Console.WriteLine($"语言资源加载完成，共加载 {count} 项。");

        // 可选：打印几个关键 key 的内容
        Console.WriteLine($"示例 MainWindow.MenuItem1: {(_resources.TryGetValue("MainWindow.MenuItem1", out var v1) ? v1 : "(未找到)")}");
        Console.WriteLine($"示例 MainWindow.MenuItem2: {(_resources.TryGetValue("MainWindow.MenuItem2", out var v2) ? v2 : "(未找到)")}");

        // 通知绑定更新
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}

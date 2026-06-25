using System.Globalization;
using System.Reflection;
using Avalonia.Platform;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;
using PropertyModels.Localization;
using RoslynPad.Models;

namespace RoslynPad.UI;

/// <summary>
/// 程序集 JSON 资源本地化服务：从嵌入的 JSON 资源文件加载多语言字符串。
/// </summary>
/// <remarks>
/// <para>
/// 此类实现 <see cref="ILocalizationService"/> 接口，提供基于 Avalonia 资源系统的本地化功能。
/// 它从程序集的 <c>Assets/Localizations</c> 目录加载 JSON 格式的语言文件。
/// </para>
/// <para>
/// 主要功能：
/// <list type="bullet">
///   <item><description>自动发现和加载嵌入的语言资源文件</description></item>
///   <item><description>支持运行时切换语言</description></item>
///   <item><description>支持扩展服务链，允许多个本地化源</description></item>
///   <item><description>语言切换时自动卸载旧资源以节省内存</description></item>
/// </list>
/// </para>
/// <para>
/// 语言文件命名约定：<c>{cultureName}.json</c>，如 <c>zh-CN.json</c>、<c>en-US.json</c>。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 创建本地化服务
/// var localization = new AssemblyJsonAssetLocalizationService1(typeof(App).Assembly);
/// 
/// // 切换语言
/// localization.SelectCulture("zh-CN");
/// 
/// // 获取本地化字符串
/// string text = localization["MainWindow.Title"];
/// 
/// // 监听语言变更
/// localization.OnCultureChanged += (s, e) => RefreshUI();
/// </code>
/// </example>
public class AssemblyJsonAssetLocalizationService1 : MiniReactiveObject, ILocalizationService
{
    /// <summary>
    /// 扩展本地化服务列表，用于支持多个本地化源。
    /// </summary>
    private readonly List<ILocalizationService> _extraServices = [];

    /// <summary>
    /// 当当前语言发生变化时触发。
    /// </summary>
    /// <remarks>
    /// 订阅此事件以在语言切换时刷新 UI。
    /// </remarks>
    public event EventHandler? OnCultureChanged;

    /// <summary>
    /// 获取所有可用的语言/区域数据列表。
    /// </summary>
    /// <value>
    /// 可选择的语言列表，支持选择状态跟踪。
    /// </value>
    public ISelectableList<ICultureData> AvailableCultures => _assetCultureData;

    /// <summary>
    /// 内部的语言数据可选列表。
    /// </summary>
    private readonly SelectableList<ICultureData> _assetCultureData = [];

    /// <summary>
    /// 获取或设置当前选中的语言数据。
    /// </summary>
    /// <value>
    /// 当前激活的 <see cref="ICultureData"/> 实例。
    /// </value>
    /// <remarks>
    /// 设置此属性会触发 <see cref="INotifyPropertyChanged.PropertyChanged"/> 事件。
    /// </remarks>
    public ICultureData CultureData
    {
        get => _assetCultureData.SelectedValue;
        set
        {
            if (_assetCultureData.SelectedValue != value)
            {
                _assetCultureData.SelectedValue = value;
                RaisePropertyChanged(nameof(CultureData));
            }
        }
    }

    /// <summary>
    /// 通过键获取本地化字符串。
    /// </summary>
    /// <param name="key">资源键。</param>
    /// <returns>
    /// 对应的本地化字符串；如果未找到，返回原始键。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 查找顺序：
    /// <list type="number">
    ///   <item><description>首先在扩展服务中查找</description></item>
    ///   <item><description>然后在当前语言资源中查找</description></item>
    ///   <item><description>如果都未找到，返回键本身</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public string this[string key]
    {
        get
        {
            foreach (var service in _extraServices)
            {
                var value = service[key];

                if (value.IsNotNullOrEmpty() && value != key)
                {
                    return value;
                }
            }

            if (_assetCultureData.SelectedValue[key] is { } text)
            {
                return text;
            }

            return key;
        }
    }

    /// <summary>
    /// 使用指定程序集初始化 <see cref="AssemblyJsonAssetLocalizationService1"/> 类的新实例。
    /// </summary>
    /// <param name="assembly">包含本地化资源的程序集。</param>
    /// <remarks>
    /// 资源文件应位于程序集的 <c>Assets/Localizations</c> 目录下。
    /// </remarks>
    public AssemblyJsonAssetLocalizationService1(Assembly assembly)
        : this(new Uri($"avares://{assembly.GetName().Name}/Assets/Localizations"))
    {
    }

    /// <summary>
    /// 使用指定的资源目录 URI 初始化 <see cref="AssemblyJsonAssetLocalizationService1"/> 类的新实例。
    /// </summary>
    /// <param name="assetDirectoryUri">本地化资源目录的 Avalonia 资源 URI。</param>
    /// <remarks>
    /// 此构造函数会自动扫描目录下的所有资源文件，并根据当前系统语言选择默认语言。
    /// </remarks>
    public AssemblyJsonAssetLocalizationService1(Uri assetDirectoryUri)
    {
        var assets = AssetLoader.GetAssets(assetDirectoryUri, null);

        foreach (var asset in assets)
        {
            var assetCultureData = new AssetCultureData(asset);
            AvailableCultures.Add(assetCultureData);
        }

        _assetCultureData.SelectionChanged += OnSelectionChanged;
        SelectCulture(CultureInfo.CurrentCulture.Name);
    }

    /// <summary>
    /// 获取所有可用的语言数据。
    /// </summary>
    /// <returns>
    /// 包含所有可用语言（包括扩展服务中的语言）的数组。
    /// </returns>
    /// <remarks>
    /// 如果多个服务提供相同的语言，只返回第一个。
    /// </remarks>
    public ICultureData[] GetCultures()
    {
        Dictionary<string, ICultureData> values = [];
        foreach (var i in _assetCultureData)
        {
            _ = values.TryAdd(i.Culture.Name, i);
        }

        foreach (var i in _extraServices)
        {
            var extraCultures = i.GetCultures();

            foreach (var extra in extraCultures)
            {
                _ = values.TryAdd(extra.Culture.Name, extra);
            }
        }

        return [.. values.Values];
    }

    /// <summary>
    /// 切换到指定的语言。
    /// </summary>
    /// <param name="cultureName">语言名称，如 "zh-CN"、"en-US"。</param>
    /// <remarks>
    /// <para>
    /// 如果指定的语言不存在，将回退到 "en-US"。
    /// 切换语言时会：
    /// <list type="bullet">
    ///   <item><description>卸载当前语言资源</description></item>
    ///   <item><description>加载新语言资源（如果尚未加载）</description></item>
    ///   <item><description>通知所有扩展服务切换语言</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public void SelectCulture(string cultureName)
    {
        foreach (var i in _extraServices)
        {
            i.SelectCulture(cultureName);
        }

        var cultureData = _assetCultureData.ToList().Find(x => x.Culture.Name == cultureName)
            ?? _assetCultureData.ToList().Find(x => x.Culture.Name == "en-US");

        if (cultureData != null)
        {
            if (!cultureData.IsLoaded)
            {
                _ = cultureData.Reload();
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (CultureData != null && CultureData != cultureData)
            {
                CultureData.Unload();
            }

            CultureData = cultureData;
        }
    }

    /// <summary>
    /// 处理语言选择变更事件。
    /// </summary>
    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        // 通知 XAML 绑定刷新
        RaisePropertyChanged("Item");
        RaisePropertyChanged("Item[]");

        // 通知代码订阅者刷新
        OnCultureChanged?.Invoke(sender, e);
    }

    #region 扩展服务管理

    /// <summary>
    /// 添加一个扩展本地化服务。
    /// </summary>
    /// <param name="service">要添加的本地化服务。</param>
    /// <remarks>
    /// 扩展服务的优先级高于主服务，在查找本地化字符串时会先搜索扩展服务。
    /// </remarks>
    public void AddExtraService(ILocalizationService service) => _extraServices.Add(service);

    /// <summary>
    /// 移除一个扩展本地化服务。
    /// </summary>
    /// <param name="service">要移除的本地化服务。</param>
    public void RemoveExtraService(ILocalizationService service) => _ = _extraServices.Remove(service);

    /// <summary>
    /// 获取所有扩展本地化服务。
    /// </summary>
    /// <returns>扩展服务数组。</returns>
    public ILocalizationService[] GetExtraServices() => [.. _extraServices];

    #endregion
}

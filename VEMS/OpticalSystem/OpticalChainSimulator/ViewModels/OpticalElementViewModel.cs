using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using OpticalChainSimulator.Converters.Coordinate;
using OpticalChainSimulator.Models;
// 在文件顶部增加 using:
using OpticalChainSimulator.Services;
using OpticalChainSimulator.yuanjianku;
using static OpticalChainSimulator.MainView;
// 引入调试命名空间（日志输出必备）
using System.Diagnostics;

namespace OpticalChainSimulator.ViewModels;

/// <summary>
/// 光学元件视图模型（ViewModel）
/// 核心功能：封装各类光学元件（光源/反射镜/透镜等）的属性、坐标逻辑、状态同步，
/// 实现MVVM双向绑定，支撑UI与数据模型的交互，包含坐标转换、属性变更监听、Model/ViewModel互转等核心逻辑
/// </summary>
public partial class OpticalElementViewModel : ViewModelBase
{
    /// <summary>
    /// 类型变更回调方法（MVVM Toolkit自动生成）
    /// 核心逻辑：元件类型变化时，自动更新名称前缀（兼容默认命名格式，如“反射镜 1”→“透镜 1”）
    /// </summary>
    /// <param name="value">新的元件类型</param>
    partial void OnTypeChanged(Models.OpticalElementType value)
    {
        // 当类型变化时，如果名称是“旧默认命名”，则自动更新名称前缀
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = GetDefaultNameForType(value);
            return;
        }

        // 如果原名称是“光源 1 / 反射镜 3 / 自由空间 2”这类默认格式，也自动替换前缀
        var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 &&
            IsKnownPrefix(parts[0]) &&
            int.TryParse(parts[1], out var index))
        {
            var newPrefix = GetPrefixForType(value);
            Name = $"{newPrefix} {index}";
        }
    }

    #region 1. 光学元件核心参数（可观察属性，支持MVVM双向绑定）
    // 通用损耗（0~1），1 表示无损
    [ObservableProperty]
    private double loss = 1.0;

    // 光源（Source）专属参数
    [ObservableProperty] private double power;               // 功率（W）
    [ObservableProperty] private double wavelength;          // 波长（nm）
    [ObservableProperty] private double beamWaist;          // 束腰半径（mm）
    [ObservableProperty] private double beamQualityM2;      // 光束质量因子M²

    // 反射镜（Mirror）专属参数
    [ObservableProperty] private double angle;              // 角度（°）
    [ObservableProperty] private double radiusOfCurvature;  // 曲率半径（mm）
    [ObservableProperty] private double radiusOfCurvature1; // 曲率半径1（备用参数）

    // 透镜（Lens）专属参数
    [ObservableProperty] private double focalLength;        // 焦距（mm）

    // 自由空间（FreeSpace）专属参数
    [ObservableProperty] private double length;             // 长度（mm）
    [ObservableProperty] private double refractiveIndex;    // 折射率

    // 光阑（Aperture）专属参数
    [ObservableProperty] private double apertureDiameter;   // 光阑直径（mm）
    [ObservableProperty] private int apertureShapeCode;     // 光阑形状编码（0=圆形/1=方形等）
    [ObservableProperty] private double apertureTransmission; // 光阑透过率（0~1）

    // 滤光片（Filter）专属参数
    [ObservableProperty] private double filterCenterWavelength; // 中心波长（nm）
    [ObservableProperty] private double filterBandwidth;        // 带宽（nm）
    [ObservableProperty] private double filterTransmission;     // 透过率（0~1）

    // 探测器（Detector）专属参数
    [ObservableProperty] private double detectorWidth;          // 宽度（mm）
    [ObservableProperty] private double detectorHeight;         // 高度（mm）
    [ObservableProperty] private double detectorPositionOverride; // 位置偏移（mm）
    [ObservableProperty] private string detectorResultText = string.Empty; // 探测器结果文本

    // 扩束器（BeamExpander）专属参数
    [ObservableProperty] private double expanderMagnification;   // 放大倍率
    [ObservableProperty] private double expanderInputDiameter;   // 输入直径（mm）
    [ObservableProperty] private double expanderOutputDiameter;  // 输出直径（mm）

    // ===== ScottPlot 探测器热力图参数 =====
    [ObservableProperty] private bool hasDetectorMap;            // 是否有探测器热力图数据
    [ObservableProperty] private double[,] detectorMap = new double[0, 0]; // 热力图数据矩阵
    [ObservableProperty] private double detectorMapXMin;         // 热力图X轴最小值
    [ObservableProperty] private double detectorMapXMax;         // 热力图X轴最大值
    [ObservableProperty] private double detectorMapYMin;         // 热力图Y轴最小值
    [ObservableProperty] private double detectorMapYMax;         // 热力图Y轴最大值

    /// <summary>
    /// 备用坐标点（Vector3结构体）
    /// 注：通过包装属性（Point1X/Y/Z）实现分量的单独绑定和更新通知
    /// </summary>
    public Vector3 _point1;
    /// <summary>
    /// Point1的X分量包装属性（供UI绑定）
    /// 核心逻辑：修改时创建新Vector3（避免结构体副本问题），触发属性变更通知
    /// </summary>
    public float Point1X
    {
        get => _point1.X;
        set
        {
            // 检查值是否真的改变了（避免无效更新）
            if (_point1.X != value)
            {
                // 创建一个新的 Vector3 对象来避免修改结构体的副本
                _point1 = new Vector3(value, _point1.Y, _point1.Z);
                // 通知 UI Point1X 属性已更改（如果其他地方也绑定了它）
                OnPropertyChanged(nameof(Point1X));
            }
        }
    }

    /// <summary>
    /// Point1的Y分量包装属性（供UI绑定）
    /// </summary>
    public float Point1Y
    {
        get => _point1.Y;
        set
        {
            if (_point1.Y != value)
            {
                _point1 = new Vector3(_point1.X, value, _point1.Z);
                OnPropertyChanged(nameof(Point1Y));
            }
        }
    }

    /// <summary>
    /// Point1的Z分量包装属性（供UI绑定）
    /// </summary>
    public float Point1Z
    {
        get => _point1.Z;
        set
        {
            if (_point1.Z != value)
            {
                _point1 = new Vector3(_point1.X, _point1.Y, value);
                OnPropertyChanged(nameof(Point1Z));
            }
        }
    }
    #endregion

    #region 2. 事件与委托（附加属性变更通知）
    /// <summary>
    /// 附加属性变更事件（键/值变化时触发）
    /// 用于通知外部（如ViewModel/View）附加属性的变更
    /// </summary>
    public event Action<OpticalElementViewModel, PropertyEntry>? AdditionalPropertyChanged;
    #endregion

    #region 3. 构造函数与ViewModel初始化
    /// <summary>
    /// 主窗口ViewModel实例（用于场景服务调用、状态同步）
    /// </summary>
    private readonly MainWindowViewModel _mainViewModel;

    /// <summary>
    /// 无参构造函数（初始化附加属性集合、坐标转换器）
    /// </summary>
    public OpticalElementViewModel()
    {
        _mainViewModel = null!;
        // 订阅集合变更以维护 PropertyEntry 的 PropertyChanged 订阅
        AdditionalProperties.CollectionChanged += AdditionalProperties_CollectionChanged;

        // 初始化默认坐标转换器（可替换为自定义实现）
        _coordinateConverter = new DefaultCoordinateConverter();
    }

    /// <summary>
    /// 基于模型的构造函数（从OpticalElement1模型初始化ViewModel）
    /// 核心逻辑：复制模型属性、初始化附加属性、设置默认相对坐标
    /// </summary>
    /// <param name="model">光学元件数据模型</param>
    /// <param name="mainViewModel">主窗口ViewModel（用于场景服务交互）</param>
    public OpticalElementViewModel(OpticalElement1 model, MainWindowViewModel mainViewModel) : this()
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        id = model.Id;
        type = model.Type;
        name = model.Name;
        pointp = model.Point1;
        // 默认相对坐标：在绝对坐标基础上偏移(1,1,1)
        pointp1 = new Vector3(pointp.X + 1f, pointp.Y + 1f, pointp.Z + 1f);

        // 处理额外属性（把模型的参数转为附加属性供UI编辑）
        foreach (var kv in model.Parameters1)
        {
            AdditionalProperties.Add(new PropertyEntry
            {
                Key = kv.Key,
                Value = kv.Value.ToString()
            });
        }
        _point1 = model.Point1;
    }
    #endregion

    #region 4. 名称辅助方法（静态）
    /// <summary>
    /// 根据元件类型获取名称前缀（如Source→“光源”）
    /// </summary>
    /// <param name="t">元件类型</param>
    /// <returns>中文前缀</returns>
    private static string GetPrefixForType(OpticalElementType t) =>
        t switch
        {
            OpticalElementType.Source => "光源",
            OpticalElementType.Mirror => "反射镜",
            OpticalElementType.Lens => "透镜",
            OpticalElementType.FreeSpace => "自由空间",
            OpticalElementType.Aperture => "光阑",
            OpticalElementType.Filter => "滤光片",
            OpticalElementType.Detector => "探测器",
            OpticalElementType.BeamExpander => "扩束器",
            OpticalElementType.Box => "Box",
            OpticalElementType.PortBox => "PortBox",
            _ => "元件"
        };

    /// <summary>
    /// 判断字符串是否为已知的元件名称前缀
    /// </summary>
    /// <param name="s">待判断的前缀</param>
    /// <returns>是否为已知前缀</returns>
    private static bool IsKnownPrefix(string s) =>
        s is "光源" or "反射镜" or "透镜" or "自由空间" or "光阑" or "滤光片" or "探测器" or "扩束器" or "元件";

    /// <summary>
    /// 根据元件类型生成默认名称（如“光源 1”）
    /// </summary>
    /// <param name="t">元件类型</param>
    /// <returns>默认名称</returns>
    private static string GetDefaultNameForType(OpticalElementType t)
    {
        return GetPrefixForType(t) + " 1";
    }
    #endregion

    #region 5. Model/ViewModel互转（核心：数据持久化/场景交互）
    /// <summary>
    /// ViewModel转OpticalElement1模型（新版模型）
    /// 核心逻辑：复制基础属性、解析附加属性为JSON/数值，供场景服务使用
    /// </summary>
    /// <returns>OpticalElement1模型实例</returns>
    public OpticalElement1 ToModel1()
    {
        var m = new OpticalElement1
        {
            // 生成唯一ID（无ID时新建，有ID时复用）
            Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
            Type = Type,
            Name = Name,
            Point1 = Pointp,
        };

        // 处理附加属性：解析为数值/JSON并写入模型参数
        foreach (var p in AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(p.Key))
                continue;

            var jsonEl = TryParseToJsonElement(p.Value);
            if (jsonEl.HasValue)
            {
                var je = jsonEl.Value;
                // 如果是纯数字类型并且是可以转换为 double，则写入 Parameters（兼容数值语义）
                if (je.ValueKind == JsonValueKind.Number && je.TryGetDouble(out var d))
                {
                    m.Parameters1[p.Key] = d;
                }
            }
        }

        return m;
    }

    /// <summary>
    /// 尝试将字符串解析为JsonElement（兼容数值/布尔/字符串/对象）
    /// 核心逻辑：先尝试直接解析，失败则包装为字符串解析，保证兼容性
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns>解析后的JsonElement（null表示解析失败）</returns>
    private static JsonElement? TryParseToJsonElement(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        // 首先尝试直接解析 raw（如果 raw 是 "3.0" / "true" / "{}" / "123"），则直接得到 JsonElement
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.Clone();
        }
        catch
        {
            // 尝试把它作为字符串值解析（加引号）
            try
            {
                var wrapped = JsonSerializer.Serialize(raw);
                using var doc2 = JsonDocument.Parse(wrapped);
                return doc2.RootElement.Clone();
            }
            catch
            {
                return null;
            }
        }
    }
    #endregion

    #region 6. 弃用的链表添加相关逻辑（保留代码，标注弃用）
    // 注：以下为旧版本Model互转逻辑，当前已弃用，保留仅作兼容参考
    public OpticalElement ToModel()
    {
        var m = new OpticalElement
        {
            Id = Id == Guid.Empty ? Guid.NewGuid() : Id,
            Type = Type,
            Name = Name,
        };

        // 辅助方法：有效数值才写入模型参数（排除NaN）
        void SetIfValid(string key, double value)
        {
            if (!double.IsNaN(value))
                m.Parameters[key] = value;
        }

        // 写入各类元件参数
        SetIfValid("Loss", Loss);
        SetIfValid("Power", Power);
        SetIfValid("Wavelength", Wavelength);
        SetIfValid("BeamWaist", BeamWaist);
        SetIfValid("BeamQualityM2", BeamQualityM2);
        SetIfValid("Angle", Angle);
        SetIfValid("RadiusOfCurvature", RadiusOfCurvature);
        SetIfValid("FocalLength", FocalLength);
        SetIfValid("Length", Length);
        SetIfValid("RefractiveIndex", RefractiveIndex);
        SetIfValid("ApertureDiameter", ApertureDiameter);
        SetIfValid("ApertureShapeCode", ApertureShapeCode);
        SetIfValid("ApertureTransmission", ApertureTransmission);
        SetIfValid("FilterCenterWavelength", FilterCenterWavelength);
        SetIfValid("FilterBandwidth", FilterBandwidth);
        SetIfValid("FilterTransmission", FilterTransmission);
        SetIfValid("DetectorWidth", DetectorWidth);
        SetIfValid("DetectorHeight", DetectorHeight);
        SetIfValid("DetectorPositionOverride", DetectorPositionOverride);
        SetIfValid("ExpanderMagnification", ExpanderMagnification);
        SetIfValid("ExpanderInputDiameter", ExpanderInputDiameter);
        SetIfValid("ExpanderOutputDiameter", ExpanderOutputDiameter);

        // 处理附加属性：解析为数值/JSON并写入模型
        foreach (var p in AdditionalProperties)
        {
            if (string.IsNullOrWhiteSpace(p.Key))
                continue;

            var jsonEl = TryParseToJsonElement(p.Value);
            if (jsonEl.HasValue)
            {
                var je = jsonEl.Value;
                if (je.ValueKind == JsonValueKind.Number && je.TryGetDouble(out var d))
                {
                    m.Parameters[p.Key] = d;
                }
                else
                {
                    m.Properties[p.Key] = je;
                }
            }
        }

        // 写入坐标模式（UsePrevious）到模型属性
        try
        {
            if (usePrevious)
            {
                using var doc = JsonDocument.Parse("\"Previous\"");
                m.Properties["Reference"] = doc.RootElement.Clone();
            }
        }
        catch { }
        return m;
    }
    #endregion

    #region 7. 属性变更监听（调试+状态同步）
    /// <summary>
    /// 附加属性项（PropertyEntry）变更事件处理方法
    /// 核心逻辑：监听附加属性的键/值变化，输出调试日志，触发外部变更通知
    /// </summary>
    /// <param name="sender">事件发送者（PropertyEntry实例）</param>
    /// <param name="e">属性变更参数</param>
    private void OnPropertyEntryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is PropertyEntry pe)
        {
            if (e.PropertyName == nameof(PropertyEntry.Value))
            {
                // 打印值变更信息（调试用）
                Debug.WriteLine($"[附加属性值变更] 元素ID: {Id} - 键: {pe.Key}, 新值: {pe.Value}");
                AdditionalPropertyChanged?.Invoke(this, pe);
            }
            else if (e.PropertyName == nameof(PropertyEntry.Key))
            {
                // 打印键变更信息（调试用）
                Debug.WriteLine($"[附加属性键变更] 元素ID: {Id} - 旧键 -> 新键: {pe.Key}");
            }
        }
    }

    /// <summary>
    /// 附加属性集合变更事件处理方法
    /// 核心逻辑：监听集合增删，自动订阅/取消订阅PropertyEntry的变更事件，输出调试日志
    /// </summary>
    /// <param name="sender">事件发送者（ObservableCollection）</param>
    /// <param name="e">集合变更参数</param>
    private void AdditionalProperties_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (PropertyEntry pe in e.NewItems)
            {
                pe.PropertyChanged += OnPropertyEntryChanged;
                // 打印新增属性信息（调试用）
                Debug.WriteLine($"[附加属性集合变更] 元素ID: {Id} 新增属性 - 键: {pe.Key}, 初始值: {pe.Value}");
            }
        }
        if (e.OldItems != null)
        {
            foreach (PropertyEntry pe in e.OldItems)
            {
                pe.PropertyChanged -= OnPropertyEntryChanged;
                // 打印移除属性信息（调试用）
                Debug.WriteLine($"[附加属性集合变更] 元素ID: {Id} 移除属性 - 键: {pe.Key}, 移除时值: {pe.Value}");
            }
        }
    }

    /// <summary>
    /// 名称变更回调方法（MVVM Toolkit自动生成）
    /// 核心逻辑：输出名称变更日志，便于调试
    /// </summary>
    /// <param name="value">新名称</param>
    partial void OnNameChanged(string value)
    {
        Debug.WriteLine($"[元素属性变更] ID: {Id}, Name 已更新为: '{value}'");
    }

    /// <summary>
    /// 绝对坐标变更回调方法（MVVM Toolkit自动生成）
    /// 核心逻辑：同步绝对坐标到相对坐标，更新场景服务中的元件数据，输出调试日志
    /// </summary>
    /// <param name="oldValue">旧绝对坐标</param>
    /// <param name="newValue">新绝对坐标</param>
    partial void OnPointpChanged(Vector3 oldValue, Vector3 newValue)
    {
        // 调用封装的同步逻辑（绝对→相对）
        SyncAbsoluteToRelative(newValue);
        // 同步更新模型并通知场景服务刷新
        ToModel1();
        Debug.WriteLine("");
        Debug.WriteLine(ToModel1());
        Debug.WriteLine("OnPointpChanged更新之后的ToModel1().Point1");
        Debug.WriteLine(ToModel1().Point1);
        Debug.WriteLine("OnPointpChanged开始更新UpdateElement");
        _mainViewModel.SceneService?.UpdateElement(ToModel1());
        Debug.WriteLine("OnPointpChanged更新之后打印SceneService");
        Debug.WriteLine(_mainViewModel.SceneService?.ToString());
        // 输出坐标变更日志（调试用）
        Debug.WriteLine($"[元素属性变更] ID: {Id}, Pointp 已更新。旧值: ({oldValue.X}, {oldValue.Y}, {oldValue.Z}), 新值: ({newValue.X}, {newValue.Y}, {newValue.Z})");
    }

    /// <summary>
    /// 相对坐标变更回调方法（MVVM Toolkit自动生成）
    /// 核心逻辑：同步相对坐标到绝对坐标，输出调试日志
    /// </summary>
    /// <param name="oldValue">旧相对坐标</param>
    /// <param name="newValue">新相对坐标</param>
    partial void OnPointp1Changed(Vector3 oldValue, Vector3 newValue)
    {
        // 调用封装的同步逻辑（相对→绝对）
        SyncRelativeToAbsolute(newValue);
        // 输出坐标变更日志（调试用）
        Debug.WriteLine($"[元素属性变更] ID: {Id}, Pointp1 已更新。旧值: ({oldValue.X}, {oldValue.Y}, {oldValue.Z}), 新值: ({newValue.X}, {newValue.Y}, {newValue.Z})");
    }

    /// <summary>
    /// 坐标模式变更回调方法（MVVM Toolkit自动生成）
    /// 核心逻辑：输出坐标模式变更日志，便于调试
    /// </summary>
    /// <param name="value">新坐标模式（true=相对坐标，false=绝对坐标）</param>
    partial void OnUsePreviousChanged(bool value)
    {
        Debug.WriteLine($"坐标模式变更：UsePrevious = {value}");
    }
    #endregion

    #region 8. 元件核心属性（链表项基础属性）
    /// <summary>
    /// 坐标转换策略（可替换，遵循接口隔离原则）
    /// 核心：解耦坐标转换算法，支持自定义转换逻辑
    /// </summary>
    private ICoordinateConverter _coordinateConverter;

    /// <summary>
    /// 坐标模式（true=相对坐标，false=绝对坐标）
    /// </summary>
    [ObservableProperty]
    public bool usePrevious = false;

    /// <summary>
    /// 主参数摘要（供UI快速展示元件核心参数）
    /// 核心逻辑：根据元件类型返回格式化的参数字符串
    /// </summary>
    public string MainParameterSummary
    {
        get
        {
            switch (Type)
            {
                case OpticalElementType.Source:
                    return $"P={Power} W, λ={Wavelength} nm";
                case OpticalElementType.Mirror:
                    return $"Angle={Angle}°";
                case OpticalElementType.Lens:
                    return $"F={FocalLength} mm";
                case OpticalElementType.FreeSpace:
                    return $"L={Length} mm, n={RefractiveIndex}";
                case OpticalElementType.Aperture:
                    return $"D={ApertureDiameter} mm, T={ApertureTransmission}";
                case OpticalElementType.Filter:
                    return $"λ₀={FilterCenterWavelength} nm, T={FilterTransmission}";
                case OpticalElementType.Detector:
                    return $"Size={DetectorWidth}×{DetectorHeight} mm";
                case OpticalElementType.BeamExpander:
                    return $"M={ExpanderMagnification}";
                default:
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// 附加属性集合（供UI动态编辑的键值对）
    /// </summary>
    public ObservableCollection<PropertyEntry> AdditionalProperties { get; } = new();
    /// <summary>
    /// 备用附加属性集合（预留扩展）
    /// </summary>
    public ObservableCollection<PropertyEntry> AdditionalProperties1 { get; } = new();

    #region 8.1 绝对坐标（Vector3 + 分量包装属性）
    /// <summary>
    /// 绝对坐标（核心坐标值，Vector3结构体）
    /// </summary>
    [ObservableProperty]
    public Vector3 pointp;

    /// <summary>
    /// 绝对坐标X分量（包装属性，供UI绑定）
    /// 核心逻辑：修改时创建新Vector3（避免结构体副本问题），触发属性变更通知
    /// </summary>
    public float PointpX
    {
        get => Pointp.X;
        set
        {
            if (Pointp.X != value)
            {
                Pointp = new Vector3(value, Pointp.Y, Pointp.Z);
                OnPropertyChanged(nameof(PointpX));
            }
        }
    }

    /// <summary>
    /// 绝对坐标Z分量（包装属性，供UI绑定）
    /// </summary>
    public float PointpZ
    {
        get => Pointp.Z;
        set
        {
            if (Pointp.Z != value)
            {
                Pointp = new Vector3(Pointp.X, Pointp.Y, value);
                OnPropertyChanged(nameof(PointpZ));
            }
        }
    }

    /// <summary>
    /// 绝对坐标Y分量（包装属性，供UI绑定）
    /// </summary>
    public float PointpY
    {
        get => Pointp.Y;
        set
        {
            if (Pointp.Y != value)
            {
                Pointp = new Vector3(Pointp.X, value, Pointp.Z);
                OnPropertyChanged(nameof(PointpY));
            }
        }
    }
    #endregion

    #region 8.2 相对坐标（Vector3 + 分量包装属性）
    /// <summary>
    /// 相对坐标（核心坐标值，Vector3结构体）
    /// </summary>
    [ObservableProperty]
    public Vector3 pointp1;

    /// <summary>
    /// 相对坐标X分量（包装属性，供UI绑定）
    /// </summary>
    public float Pointp1X
    {
        get => Pointp1.X;
        set
        {
            if (Pointp1.X != value)
            {
                Pointp1 = new Vector3(value, Pointp1.Y, Pointp1.Z);
                OnPropertyChanged(nameof(Pointp1X));
            }
        }
    }

    /// <summary>
    /// 相对坐标Z分量（包装属性，供UI绑定）
    /// </summary>
    public float Pointp1Z
    {
        get => Pointp1.Z;
        set
        {
            if (Pointp1.Z != value)
            {
                Pointp1 = new Vector3(Pointp1.X, Pointp1.Y, value);
                OnPropertyChanged(nameof(Pointp1Z));
            }
        }
    }

    /// <summary>
    /// 相对坐标Y分量（包装属性，供UI绑定）
    /// </summary>
    public float Pointp1Y
    {
        get => Pointp1.Y;
        set
        {
            if (Pointp1.Y != value)
            {
                Pointp1 = new Vector3(Pointp1.X, value, Pointp1.Z);
                OnPropertyChanged(nameof(Pointp1Y));
            }
        }
    }
    #endregion

    /// <summary>
    /// 元件名称（供UI展示/编辑）
    /// </summary>
    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// 折叠/展开状态（UI参数面板的显示控制）
    /// </summary>
    [ObservableProperty]
    private bool isExpanded = false;

    /// <summary>
    /// 元件唯一标识（Guid，用于场景服务定位元件）
    /// </summary>
    [ObservableProperty]
    private Guid id;

    /// <summary>
    /// 元件类型（光源/反射镜/透镜等）
    /// </summary>
    [ObservableProperty]
    private Models.OpticalElementType type;

    /// <summary>
    /// 坐标同步抑制标记（避免循环触发同步）
    /// 核心：防止绝对/相对坐标互转时无限循环更新
    /// </summary>
    private bool _suppressCoordinateSync = false;
    #endregion

    #region 9. 坐标同步逻辑（封装核心算法）
    /// <summary>
    /// 绝对坐标转相对坐标（封装同步逻辑，防止循环触发）
    /// </summary>
    /// <param name="absoluteCoord">新的绝对坐标</param>
    private void SyncAbsoluteToRelative(Vector3 absoluteCoord)
    {
        if (_suppressCoordinateSync)
        {
            Debug.WriteLine("[坐标同步] SyncAbsoluteToRelative: 被抑制 (suppress)，不触发再次同步。");
            return;
        }

        try
        {
            _suppressCoordinateSync = true;
            // 调用策略接口转换坐标（核心：算法替换点）
            Pointp1 = _coordinateConverter.AbsoluteToRelative(absoluteCoord);

            // 通知UI刷新相对坐标分量
            OnPropertyChanged(nameof(Pointp1X));
            OnPropertyChanged(nameof(Pointp1Y));
            OnPropertyChanged(nameof(Pointp1Z));
        }
        finally
        {
            _suppressCoordinateSync = false;
        }
    }

    /// <summary>
    /// 相对坐标转绝对坐标（封装同步逻辑，防止循环触发）
    /// </summary>
    /// <param name="relativeCoord">新的相对坐标</param>
    private void SyncRelativeToAbsolute(Vector3 relativeCoord)
    {
        if (_suppressCoordinateSync)
        {
            Debug.WriteLine("[坐标同步] SyncRelativeToAbsolute: 被抑制 (suppress)，不触发再次同步。");
            return;
        }

        try
        {
            _suppressCoordinateSync = true;
            // 调用策略接口转换坐标（核心：算法替换点）
            Pointp = _coordinateConverter.RelativeToAbsolute(relativeCoord);

            // 通知UI刷新绝对坐标分量
            OnPropertyChanged(nameof(PointpX));
            OnPropertyChanged(nameof(PointpY));
            OnPropertyChanged(nameof(PointpZ));
        }
        finally
        {
            _suppressCoordinateSync = false;
        }
    }
    #endregion
}

/// <summary>
/// 附加属性键值对（供UI绑定并可编辑）
/// 核心功能：实现INotifyPropertyChanged，支持键/值变更通知，支撑动态属性编辑
/// </summary>
public class PropertyEntry : INotifyPropertyChanged
{
    private string _key = string.Empty;
    private string _value = string.Empty;

    /// <summary>
    /// 附加属性键（唯一标识）
    /// </summary>
    public string Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 附加属性值（可编辑的字符串）
    /// </summary>
    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// 属性变更事件（实现INotifyPropertyChanged必备）
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发属性变更通知（支持CallerMemberName，自动获取属性名）
    /// </summary>
    /// <param name="name">属性名（默认自动获取）</param>
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
namespace OpticalChainSimulator.ViewModels;

using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using OpticalChainSimulator.Factories;
using OpticalChainSimulator.Models;
using OpticalChainSimulator.Services.ComponentServices;
using OpticalChainSimulator.Services.Dialog;
using OpticalChainSimulator.Services.SceneService;
using OpticalChainSimulator.Services.Theme;
using OpticalChainSimulator.ViewModels.Docks;
using OpticalChainSimulator.yuanjianku;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using static OpticalChainSimulator.MainView;

/// <summary>
/// 主窗口视图模型（核心VM）
/// 统筹整个光学仿真应用的核心逻辑：工程管理（新建/加载/保存/关闭）、元件库管理、3D场景同步、
/// 光学链路编辑（增删/拖拽排序）、仿真运行、布局管理、主题切换等，遵循MVVM模式解耦视图与业务逻辑。
/// 保留原有核心逻辑，已移除对MainView的直接引用，通过接口/服务实现解耦。
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// 构造函数：初始化主窗口VM核心资源
    /// </summary>
    /// <param name="dialogService">对话框服务（用于弹出"关于"等对话框）</param>
    public MainWindowViewModel(IDialogService? dialogService)
    {
        // 初始化对话框服务
        _dialogService = dialogService;

        // 初始化元件库集合（左侧元件库展示用）
        ElementLibraryA = new ObservableCollection<OpticalElement1>();
        // 加载预设元件库数据（模拟从文件/数据库加载）
        LoadElementLibrary();

        // 订阅链路集合变更事件：链表增删时同步更新3D场景
        elements.CollectionChanged += Elements_CollectionChanged;
    }

    // ==================== 事件定义（跨组件通信） ====================
    /// <summary>
    /// 元件移除事件（通知3D场景删除对应几何体）
    /// </summary>
    public event Action<Guid>? ElementRemoved;

    /// <summary>
    /// 工程关闭事件（通知外部组件清理资源）
    /// </summary>
    public event Action? ProjectClosed;

    /// <summary>
    /// 关闭元件库弹窗并返回选中元件的请求事件
    /// </summary>
    public event Action<OpticalElement1?>? RequestClose;

    #region 1. 链表拖动排序相关（备用逻辑）
    /// <summary>
    /// 拖动调整链路中元件的顺序
    /// </summary>
    /// <param name="oldIndex">原索引</param>
    /// <param name="newIndex">新索引</param>
    public void MoveElement(int oldIndex, int newIndex)
    {
        // 边界校验：索引无效/未变化时直接返回
        if (oldIndex == newIndex ||
            oldIndex < 0 || oldIndex >= Elements.Count ||
            newIndex < 0 || newIndex > Elements.Count)
            return;

        // 取出待移动的元件VM
        var item = Elements[oldIndex];
        // 从原位置移除
        Elements.RemoveAt(oldIndex);

        // 修正新索引（移除后集合长度变化）
        if (oldIndex < newIndex)
            newIndex--;
        // 二次边界校验
        if (newIndex < 0)
            newIndex = 0;
        if (newIndex > Elements.Count)
            newIndex = Elements.Count;

        // 插入到新位置
        Elements.Insert(newIndex, item);

        // 更新选中状态
        SelectedElement = item;
        SelectedIndex = newIndex;
    }

    /// <summary>
    /// 刷新选中元件的参数文本（供UI展示）
    /// </summary>
    public void RefreshSelectedParameters()
    {
        OnPropertyChanged(nameof(SelectedParametersText));
    }

    /// <summary>
    /// 选中元件的参数文本（格式化展示）
    /// </summary>
    public string SelectedParametersText =>
        SelectedElement == null
            ? "未选择元件"
            : string.Join(Environment.NewLine,
                SelectedElement.ToModel().Parameters.Select(p => $"{p.Key} = {p.Value}"));

    /// <summary>
    /// 链路中选中元件的索引（双向绑定UI）
    /// </summary>
    [ObservableProperty]
    private int selectedIndex;

    /// <summary>
    /// 链路中选中的元件VM（双向绑定UI）
    /// </summary>
    [ObservableProperty]
    private OpticalElementViewModel selectedElement;

    /// <summary>
    /// 选中元件变更回调（MVVM Toolkit自动生成）
    /// 核心逻辑：选中元件时通知3D场景高亮对应几何体
    /// </summary>
    /// <param name="oldValue">旧选中元件</param>
    /// <param name="newValue">新选中元件</param>
    partial void OnSelectedElementChanged(OpticalElementViewModel? oldValue, OpticalElementViewModel? newValue)
    {
        // 调试日志：输出选中状态变更
        Debug.WriteLine($"SelectedElement 发生变化：");
        Debug.WriteLine($"旧值：{oldValue?.Name ?? "null"}");
        Debug.WriteLine($"新值：{newValue?.Name ?? "null"}");

        // 场景服务未初始化时直接返回
        if (SceneService == null) return;

        // 新选中元件不为空时，通知场景高亮
        if (newValue != null)
            SceneService.HighlightElement(newValue.Id);
    }

    /// <summary>
    /// 供外部调用的选中元件入口（通过Guid定位）
    /// </summary>
    public Guid? Poin
    {
        get => null;
        set => ToselectedElement(value);
    }

    /// <summary>
    /// 根据元件ID定位并选中链路中的元件
    /// </summary>
    /// <param name="elementId">元件唯一标识</param>
    /// <exception cref="ArgumentException">ID为Guid.Empty时抛出</exception>
    public void ToselectedElement(Guid? elementId)
    {
        // 调试日志：输出集合信息，确认是否为目标集合
        Debug.WriteLine($"ToselectedElement - Elements实例地址: {Elements.GetHashCode()}");
        Debug.WriteLine($"ToselectedElement - Elements数量: {Elements.Count}");

        // 1. 入参校验：排除无效的空Guid
        if (elementId == Guid.Empty)
        {
            throw new ArgumentException("元件ID无效，不能是Guid.Empty", nameof(elementId));
        }

        // 2. 从链路集合中查找匹配ID的元件
        var targetElement = this.Elements.FirstOrDefault(e => e.Id == elementId);

        // 3. 更新选中状态
        if (targetElement != null)
        {
            SelectedElement = targetElement;
        }

        // 4. 调试日志：反馈查找结果
        if (targetElement != null)
        {
            Console.WriteLine($"成功找到元件：ID={elementId}，名称={targetElement.Name}，已赋值给SelectedElement");
        }
        else
        {
            Console.WriteLine($"未找到ID为 {elementId} 的元件，SelectedElement已置为null");
        }
    }

    public Guid? Poin1
    {
        get => null;
        set => ToselectedElement1(value);
    }

    /// <summary>
    /// 根据元件ID定位并选中链路中的元件（附带展开/折叠参数面板）
    /// </summary>
    /// <param name="elementId">元件唯一标识</param>
    public void ToselectedElement1(Guid? elementId)
    {
        // 调试日志：输出集合信息
        Debug.WriteLine($"ToselectedElement1 - Elements实例地址: {Elements.GetHashCode()}");
        Debug.WriteLine($"ToselectedElement1 - Elements数量: {Elements.Count}");

        // 入参校验
        if (elementId == Guid.Empty)
        {
            throw new ArgumentException("元件ID无效，不能是Guid.Empty", nameof(elementId));
        }

        // 查找目标元件
        var targetElement = this.Elements.FirstOrDefault(e => e.Id == elementId);

        // 更新选中状态并切换展开/折叠
        if (targetElement != null)
        {
            SelectedElement = targetElement;
            SelectedElement.IsExpanded = !SelectedElement.IsExpanded;
        }

        // 调试日志
        if (targetElement != null)
        {
            Console.WriteLine($"成功找到元件：ID={elementId}，名称={targetElement.Name}，已赋值给SelectedElement并切换展开状态");
        }
        else
        {
            Console.WriteLine($"未找到ID为 {elementId} 的元件，SelectedElement已置为null");
        }
    }

    /// <summary>
    /// 备用选中元件字段（未启用）
    /// </summary>
    private OpticalElementViewModel _selectedElement1;
    #endregion

    #region 2. 导出参数（链路参数文本化）
    /// <summary>
    /// 导出的链路参数文本（供UI展示）
    /// </summary>
    [ObservableProperty]
    private string exportedText = string.Empty;

    /// <summary>
    /// 导出链路中所有元件的参数到文本
    /// </summary>
    [RelayCommand]
    private void ExportParameters()
    {
        var sb = new StringBuilder();

        // 遍历链路，格式化每个元件的参数
        for (int i = 0; i < Elements.Count; i++)
        {
            var el = Elements[i].ToModel();
            sb.AppendLine($"Index {i}:  {el.Type} ({el.Name})");

            foreach (var kv in el.Parameters)
                sb.AppendLine($"  {kv.Key} = {kv.Value}");
        }

        // 更新导出文本（触发UI刷新）
        ExportedText = sb.ToString();
    }
    #endregion

    #region 3. 3D场景服务（核心：链路与3D画布同步）
    /// <summary>
    /// 3D场景服务（负责3D几何体的增删改查、高亮等操作）
    /// </summary>
    public ISceneService? SceneService
    {
        get => _sceneService;
        set
        {
            // 避免重复赋值
            if (_sceneService == value) return;

            // 更新场景服务实例
            _sceneService = value;

            // 场景服务初始化后，同步现有链路到3D场景
            if (_sceneService != null)
            {
                try
                {
                    // 清空场景原有几何体
                    _sceneService.ClearAll(updateScene: false);
                    // 遍历链路，逐个添加到3D场景
                    foreach (var vmEl in Elements)
                    {
                        var template = vmEl.ToModel();
                        // 根据元件类型调用对应添加方法
                        switch (template.Type)
                        {
                            case OpticalElementType.Source:
                                _sceneService.AddSource(template, notifyVm: false);
                                break;
                            case OpticalElementType.Mirror:
                                _sceneService.AddMirror(template, notifyVm: false);
                                break;
                            case OpticalElementType.Lens:
                                _sceneService.AddLens(template, notifyVm: false);
                                break;
                            case OpticalElementType.Detector:
                                _sceneService.AddDet(template, notifyVm: false);
                                break;
                        }
                    }
                    // 刷新3D场景列表
                    _sceneService.RefreshListPublic(false);
                }
                catch (Exception ex)
                {
                    // 异常日志：场景初始化同步失败
                    Debug.WriteLine($"SceneService initial sync failed: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// 3D场景服务私有字段
    /// </summary>
    private ISceneService? _sceneService;
    #endregion

    #region 4. 元件库管理（左侧元件库）
    /// <summary>
    /// 元件库集合（左侧展示的预设元件列表）
    /// </summary>
    public ObservableCollection<OpticalElement1> ElementLibraryA { get; }

    /// <summary>
    /// 加载预设元件库数据（模拟从文件/数据库加载）
    /// </summary>
    private void LoadElementLibrary()
    {
        // 反射镜
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 1,
            Name = "反射镜",
            Icon = "🪞",
            Type = OpticalElementType.Mirror,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLength1", 50.0 },
                { "FocalLength9", 50.0 },
                { "FocalLength8", 50.0 },
                { "FocalLength7", 50.0 },
                { "FocalLength6", 50.0 }
            },
            Point1 = new Vector3(4, 4, 4)
        });
        // 透镜
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 2,
            Name = "透镜",
            Icon = "🔍",
            Type = OpticalElementType.Lens,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLengt2h", 50.0 }
            },
            Point1 = new Vector3(3, 3, 3)
        });
        // 光源
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 3,
            Name = "光源",
            Icon = "☀",
            Type = OpticalElementType.Source,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLength3", 50.0 }
            }
        });
        // 探测器
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 4,
            Name = "探测器",
            Icon = "📡",
            Type = OpticalElementType.Detector,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLength4", 50.0 }
            }
        });
        // 分束器
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 5,
            Name = "分束器",
            Icon = "➗",
            Type = OpticalElementType.BeamExpander,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLength5", 50.0 }
            }
        });
        // 棱镜
        ElementLibraryA.Add(new OpticalElement1
        {
            Id1 = 6,
            Name = "棱镜",
            Icon = "🔮",
            Type = OpticalElementType.BeamExpander,
            Parameters1 = new Dictionary<string, double>
            {
                { "FocalLength6", 50.0 }
            }
        });
    }

    /// <summary>
    /// 点击元件库按钮时的命令（触发添加元件逻辑）
    /// </summary>
    /// <param name="element">选中的元件库元件</param>
    [RelayCommand]
    public void AddElement(OpticalElement1 element)
    {
        // 调试日志：输出待添加元件信息
        Debug.WriteLine($"准备添加元件: {element.Icon} {element.Name} (ID: {element.Id})");
        try
        {
            // 触发关闭弹窗并返回元件的事件
            RequestClose?.Invoke(element);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RequestClose handler threw: {ex}");
        }
    }

    /// <summary>
    /// 打开元件库对话框（弹窗）
    /// </summary>
    /// <param name="owner">父窗口（用于居中显示弹窗）</param>
    [RelayCommand]
    public async Task OpenLibrary(object? owner)
    {
        Window? ownerWindow = owner as Window;

        // 自动获取主窗口作为父窗口（未传入时）
        if (ownerWindow == null)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ownerWindow = desktop.MainWindow;
            }
        }

        // 打开元件库对话框
        await OpenLibraryDialogAsync(ownerWindow);
    }

    /// <summary>
    /// 打开元件库对话框并处理选中结果
    /// </summary>
    /// <param name="owner">父窗口</param>
    public async Task OpenLibraryDialogAsync(Window owner)
    {
        // 创建元件库对话框实例
        var dialog = new Views.LibraryDialog(this)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        try
        {
            // 继承父窗口图标
            dialog.Icon = owner?.Icon;
        }
        catch { }

        // 绑定VM到对话框
        dialog.DataContext = this;
        // 显示对话框并获取选中的元件
        var selectedElement = await dialog.ShowDialog<OpticalElement1?>(owner);
        // 添加选中的元件到链路
        AddFromLibrary1(selectedElement);
    }

    /// <summary>
    /// 从元件库添加元件到链路
    /// </summary>
    /// <param name="element">选中的元件库元件</param>
    public void AddFromLibrary1(OpticalElement1? element)
    {
        // 空值校验
        if (element == null) return;

        // 深拷贝元件（避免修改库原件）
        var modelCopy = new OpticalElement1
        {
            Id = element.Id == Guid.Empty ? Guid.NewGuid() : element.Id,
            Type = element.Type,
            Name = element.Name,
            Parameters1 = new Dictionary<string, double>(element.Parameters1),
            Point1 = element.Point1,
        };

        // 创建元件VM并添加到链路
        var vm = new OpticalElementViewModel(modelCopy, this);
        Elements.Add(vm);

        // 更新选中状态（选中新增的元件）
        SelectedElement = vm;
        SelectedIndex = Elements.Count - 1;

        // 调试日志：输出链路所有元件信息
        if (elements != null)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                var currentElement = elements[i];
                Debug.WriteLine($"--- 索引: {i} ---");
                Debug.WriteLine($"元素: {currentElement}");
                Debug.WriteLine($"元素名称: {currentElement.Name}");
                Debug.WriteLine($"元素ID: {currentElement.Id}");
                Debug.WriteLine($"元素类型: {currentElement.Type}");
                Debug.WriteLine($"附加属性: {currentElement.AdditionalProperties}");
                Debug.WriteLine($"备用坐标: {currentElement._point1}");
                Debug.WriteLine("----------------");

                // 示例：对第6个元素（索引5）做特殊处理
                if (i == 5)
                {
                    Debug.WriteLine($"正在处理第6个元素: {currentElement.Name}");
                }
            }
        }

        // 调试日志：确认添加结果
        Debug.WriteLine($"AddFromLibrary1 - Elements实例地址: {Elements.GetHashCode()}");
        Debug.WriteLine($"AddFromLibrary1 - Elements数量: {Elements.Count}");
    }
    #endregion

    #region 5. 工程管理（新建/加载/保存/关闭）
    /// <summary>
    /// 主窗口实例（用于文件对话框的父窗口）
    /// </summary>
    public Window? MainWindow { get; set; }

    /// <summary>
    /// 新建工程命令
    /// </summary>
    [RelayCommand]
    public void HandleNewProjectClicked1()
    {
        // 先关闭当前工程
        if (CloseProjectCommand.CanExecute(null))
            CloseProjectCommand.Execute(null);

        // 初始化新工程参数
        ProjectName = "未命名光学工程";
        ProjectDescription = "新的光学链路工程。";
        Elements.Clear();
        ExportedText = string.Empty;
        SimulationResultText = "尚未运行仿真。";
        SimulationAsciiText = string.Empty;
        SelectedIndex = -1;
        SelectedElement = null;
        CurrentProjectFilePath = string.Empty;
    }

    /// <summary>
    /// 加载工程命令（弹窗选择文件）
    /// </summary>
    [RelayCommand]
    public async Task HandleLoadProjectClickedAsync1()
    {
        // 主窗口未初始化时返回
        if (MainWindow == null) return;

        // 创建打开文件对话框
        var ofd = new OpenFileDialog
        {
            Title = "打开光学工程",
            AllowMultiple = false,
            Filters =
            {
                new FileDialogFilter { Name = "Optical Project", Extensions = { "optproj" } },
                new FileDialogFilter { Name = "All Files", Extensions = { "*" } }
            }
        };

        // 显示对话框并获取选中路径
        var paths = await ofd.ShowAsync(MainWindow);
        var path = paths?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(path))
        {
            // 关闭当前工程后加载新工程
            if (CloseProjectCommand.CanExecute(null))
                CloseProjectCommand.Execute(null);

            LoadProjectCommand.Execute(path);
        }
    }

    /// <summary>
    /// 保存工程命令（弹窗选择保存路径）
    /// </summary>
    [RelayCommand]
    public async Task HandleSaveProjectClickedAsync1()
    {
        // 主窗口未初始化时返回
        if (MainWindow == null) return;

        // 创建保存文件对话框
        var sfd = new SaveFileDialog
        {
            Title = "保存光学工程",
            Filters =
            {
                new FileDialogFilter { Name = "Optical Project", Extensions = { "optproj" } },
                new FileDialogFilter { Name = "All Files", Extensions = { "*" } }
            },
            DefaultExtension = "optproj"
        };

        // 显示对话框并获取保存路径
        var path = await sfd.ShowAsync(MainWindow);
        if (!string.IsNullOrWhiteSpace(path))
        {
            SaveProjectCommand.Execute(path);
        }
    }

    /// <summary>
    /// 保存工程核心逻辑
    /// </summary>
    /// <param name="path">保存路径</param>
    [RelayCommand]
    private void SaveProject(string? path)
    {
        // 路径为空时返回
        if (string.IsNullOrWhiteSpace(path))
            return;

        // 构建工程模型
        var project = new OpticalProject
        {
            Name = ProjectName,
            Description = ProjectDescription,
            Elements1 = Elements.Select(e => e.ToModel1()).ToList()
        };

        // 序列化保存工程
        ProjectSerializer.Save(path, project);
        // 更新当前工程文件路径
        CurrentProjectFilePath = path;
    }

    /// <summary>
    /// 加载工程核心逻辑
    /// </summary>
    /// <param name="path">工程文件路径</param>
    [RelayCommand]
    private void LoadProject(string? path)
    {
        // 路径为空时返回
        if (string.IsNullOrWhiteSpace(path))
            return;

        // 反序列化加载工程
        var project = ProjectSerializer.Load(path);
        if (project == null)
            return;

        // 更新工程信息
        ProjectName = project.Name;
        ProjectDescription = project.Description;

        // 清空现有链路并加载工程中的元件
        Elements.Clear();
        foreach (var m in project.Elements1)
            Elements.Add(new OpticalElementViewModel(m, this));

        // 更新选中状态
        if (Elements.Count > 0)
        {
            SelectedIndex = 0;
            SelectedElement = Elements[0];
        }
        else
        {
            SelectedIndex = -1;
            SelectedElement = null;
        }

        // 更新工程路径和UI展示
        CurrentProjectFilePath = path;
        RefreshSelectedParameters();
        ExportParameters();
        SimulationResultText = "工程已加载，请重新运行仿真。";
        SimulationAsciiText = string.Empty;
    }

    /// <summary>
    /// 关闭工程核心逻辑
    /// </summary>
    [RelayCommand]
    private void CloseProject()
    {
        try
        {
            // 清空3D场景
            if (SceneService != null)
                SceneService.ClearAll();
            // 清空链路
            Elements.Clear();
            // 重置选中状态
            SelectedIndex = -1;
            SelectedElement = null;
            // 重置仿真/导出文本
            ExportedText = string.Empty;
            SimulationResultText = "尚未运行仿真。";
            SimulationAsciiText = string.Empty;
            // 重置工程信息
            ProjectName = "未命名光学工程";
            ProjectDescription = string.Empty;
            CurrentProjectFilePath = string.Empty;
            // 触发工程关闭事件
            ProjectClosed?.Invoke();
        }
        catch (Exception ex)
        {
            // 异常日志
            Debug.WriteLine("CloseProject error: " + ex);
        }
    }

    // 工程信息属性（双向绑定UI）
    /// <summary>
    /// 工程名称
    /// </summary>
    [ObservableProperty]
    private string projectName = "示例光学工程";

    /// <summary>
    /// 工程描述
    /// </summary>
    [ObservableProperty]
    private string projectDescription = "演示光学链路编辑与参数导出和仿真的示例工程。";

    /// <summary>
    /// 当前工程文件路径
    /// </summary>
    [ObservableProperty]
    private string currentProjectFilePath = string.Empty;

    /// <summary>
    /// 仿真结果文本（供UI展示）
    /// </summary>
    [ObservableProperty]
    private string simulationResultText = "尚未运行仿真。";

    /// <summary>
    /// ASCII格式光路描述（供UI展示）
    /// </summary>
    [ObservableProperty]
    private string simulationAsciiText = string.Empty;
    #endregion

    #region 6. 链路增删（核心：编辑光学链路）
    /// <summary>
    /// 光学链路集合（可拖拽排序，双向绑定UI）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<OpticalElementViewModel> elements = new();

    /// <summary>
    /// 删除选中元件命令
    /// </summary>
    [RelayCommand]
    private void DeleteElement()
    {
        // 链路为空时重置选中状态
        if (Elements == null || Elements.Count == 0)
        {
            SelectedIndex = -1;
            SelectedElement = null;
            return;
        }

        // 修正选中索引（边界校验）
        if (SelectedIndex < 0)
            SelectedIndex = 0;
        if (SelectedIndex >= Elements.Count)
            SelectedIndex = Elements.Count - 1;

        // 记录待删除的索引和元件
        var removeIndex = SelectedIndex;
        var item = Elements[removeIndex];

        // 从链路中移除
        Elements.RemoveAt(removeIndex);

        try
        {
            // 触发元件移除事件（通知3D场景删除）
            ElementRemoved?.Invoke(item.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ElementRemoved handler threw: {ex}");
        }

        // 更新选中状态
        if (Elements.Count == 0)
        {
            SelectedIndex = -1;
            SelectedElement = null;
        }
        else
        {
            if (removeIndex >= Elements.Count)
                SelectedIndex = Elements.Count - 1;
            else
                SelectedIndex = removeIndex;

            SelectedElement = Elements[SelectedIndex];
        }
    }

    /// <summary>
    /// 删除所有元件命令
    /// </summary>
    [RelayCommand]
    private void DeleteAllElements()
    {
        // 记录所有元件ID（用于通知3D场景删除）
        var ids = Elements.Select(e => e.Id).ToList();

        // 清空链路
        Elements.Clear();

        // 重置选中状态
        SelectedIndex = -1;
        SelectedElement = null;

        // 通知3D场景删除所有元件
        if (ids.Count > 0 && ElementRemoved != null)
        {
            foreach (var id in ids)
            {
                try
                {
                    ElementRemoved?.Invoke(id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ElementRemoved handler threw during DeleteAllElements: {ex}");
                }
            }
        }
    }
    #endregion

    #region 7. 链路变更同步3D场景（核心）
    /// <summary>
    /// 链路集合变更事件处理（增删元件时同步3D场景）
    /// </summary>
    /// <param name="sender">事件发送者（Elements集合）</param>
    /// <param name="e">集合变更参数</param>
    private void Elements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // 场景服务未初始化时返回
        if (SceneService == null) return;

        // 新增元件：同步添加到3D场景
        if (e.NewItems != null)
        {
            Debug.WriteLine("e.NewItems");
            Debug.WriteLine(e.NewItems);
            foreach (OpticalElementViewModel vmEl in e.NewItems)
            {
                // 转换为模型
                var template = vmEl.ToModel();
                var template1 = vmEl.ToModel1();
                Debug.WriteLine(template1.Point1);
                // 根据类型添加到3D场景
                switch (template.Type)
                {
                    case OpticalElementType.Source:
                        SceneService.AddSource(template, notifyVm: false);
                        break;
                    case OpticalElementType.Mirror:
                        SceneService.AddMirror1(template1, notifyVm: false);
                        break;
                    case OpticalElementType.Lens:
                        SceneService.AddLens(template, notifyVm: false);
                        break;
                    case OpticalElementType.Detector:
                        SceneService.AddDet(template, notifyVm: false);
                        break;
                }

                // 刷新3D场景列表
                SceneService.RefreshListPublic(false);
            }
        }

        // 删除元件：从3D场景移除
        if (e.OldItems != null)
        {
            foreach (OpticalElementViewModel vmEl in e.OldItems)
            {
                SceneService.RemoveElementByElementId(vmEl.Id, updateScene: true);
            }
            SceneService.RefreshListPublic(false);
        }
    }
    #endregion

    #region 8. 退出应用
    /// <summary>
    /// 菜单退出事件处理（备用）
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">路由事件参数</param>
    private void OnMenuExit(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 关闭应用
            (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.Shutdown();
        }
        catch { }
    }

    /// <summary>
    /// 退出应用命令（核心）
    /// </summary>
    [RelayCommand]
    private async Task Exit()
    {
        try
        {
            // 确保在UI线程执行关闭操作
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    desktopLifetime.Shutdown();
                }
            });
        }
        catch (Exception ex)
        {
            // 异常日志（可扩展为日志框架）
            Debug.WriteLine($"退出应用时发生错误: {ex}");
        }
    }
    #endregion

    #region 9. 布局管理（Dock布局预设/保存/加载）
    /// <summary>
    /// Dock布局工厂（创建布局节点）
    /// </summary>
    [ObservableProperty]
    private IFactory? factory;

    /// <summary>
    /// 根布局节点
    /// </summary>
    [ObservableProperty]
    private IDock? layout;

    /// <summary>
    /// 应用编辑模式布局预设
    /// </summary>
    [RelayCommand]
    private void LayoutEditMode()
    {
        if (Factory is OpticStudioDockFactory factory && Layout != null)
        {
            LayoutPresets.ApplyEditMode(factory, Layout);
        }
    }

    /// <summary>
    /// 应用分析模式布局预设
    /// </summary>
    [RelayCommand]
    private void LayoutAnalysisMode()
    {
        if (Factory is OpticStudioDockFactory factory && Layout != null)
        {
            LayoutPresets.ApplyAnalysisMode(factory, Layout);
        }
    }

    /// <summary>
    /// 应用仿真模式布局预设
    /// </summary>
    [RelayCommand]
    private void LayoutSimulationMode()
    {
        if (Factory is OpticStudioDockFactory factory && Layout != null)
        {
            LayoutPresets.ApplySimulationMode(factory, Layout);
        }
    }

    /// <summary>
    /// 保存布局到文件
    /// </summary>
    [RelayCommand]
    private async Task SaveLayout()
    {
        if (MainWindow == null || Layout == null) return;

        // 创建保存文件对话框
        var sfd = new SaveFileDialog
        {
            Title = "保存布局",
            Filters = { new FileDialogFilter { Name = "Layout State", Extensions = { "json" } } }
        };

        // 显示对话框并获取路径
        var path = await sfd.ShowAsync(MainWindow);
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                // 收集布局状态
                var layoutState = new LayoutState();
                CollectLayoutState(Layout, layoutState);

                // 序列化保存
                var json = JsonSerializer.Serialize(layoutState, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Save layout failed: {ex}");
            }
        }
    }

    /// <summary>
    /// 从文件加载布局
    /// </summary>
    [RelayCommand]
    private async Task LoadLayout()
    {
        if (Factory == null || Layout == null || MainWindow == null) return;

        // 创建打开文件对话框
        var ofd = new OpenFileDialog
        {
            Title = "加载布局",
            AllowMultiple = false,
            Filters = { new FileDialogFilter { Name = "Layout State", Extensions = { "json" } } }
        };

        // 显示对话框并获取路径
        var paths = await ofd.ShowAsync(MainWindow);
        var path = paths?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                // 反序列化加载布局状态
                var json = await File.ReadAllTextAsync(path);
                var layoutState = JsonSerializer.Deserialize<LayoutState>(json);

                // 应用布局状态
                if (layoutState != null)
                {
                    ApplyLayoutState(Layout, layoutState);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load layout failed: {ex}");
            }
        }
    }

    /// <summary>
    /// 递归收集布局状态（比例等）
    /// </summary>
    /// <param name="dockable">布局节点</param>
    /// <param name="state">布局状态对象</param>
    private void CollectLayoutState(IDockable dockable, LayoutState state)
    {
        if (dockable is ProportionalDock pd)
        {
            foreach (var child in pd.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                if (!string.IsNullOrEmpty(child.Id))
                {
                    state.Proportions[child.Id] = child.Proportion;
                }
                CollectLayoutState(child, state);
            }
        }
        else if (dockable is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                CollectLayoutState(child, state);
            }
        }
    }

    /// <summary>
    /// 递归应用布局状态（比例等）
    /// </summary>
    /// <param name="dockable">布局节点</param>
    /// <param name="state">布局状态对象</param>
    private void ApplyLayoutState(IDockable dockable, LayoutState state)
    {
        if (dockable is ProportionalDock pd)
        {
            foreach (var child in pd.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                if (!string.IsNullOrEmpty(child.Id) && state.Proportions.ContainsKey(child.Id))
                {
                    child.Proportion = state.Proportions[child.Id];
                }
                ApplyLayoutState(child, state);
            }
        }
        else if (dockable is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                ApplyLayoutState(child, state);
            }
        }
    }

    /// <summary>
    /// 布局状态类（保存比例信息）
    /// </summary>
    private class LayoutState
    {
        /// <summary>
        /// 节点ID-比例映射
        /// </summary>
        public Dictionary<string, double> Proportions { get; set; } = new();
    }
    #endregion

    #region 10. "关于"对话框
    /// <summary>
    /// 对话框服务实例
    /// </summary>
    private readonly IDialogService _dialogService;

    /// <summary>
    /// 显示"关于"对话框（带ViewModel版本）
    /// </summary>
    [RelayCommand]
    private async Task ShowAboutDialogAsync()
    {
        try
        {
            // 创建关于对话框VM
            var aboutViewModel = new AboutDialogViewModel();
            // 显示对话框
            await _dialogService.ShowDialogAsync(aboutViewModel);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"显示关于对话框时发生错误: {ex}");
        }
    }

    /// <summary>
    /// 显示"关于"对话框（简化版）
    /// </summary>
    [RelayCommand]
    private async Task ShowAboutDialogAsync1()
    {
        try
        {
            await _dialogService.ShowAboutDialogAsync();
        }
        catch { }
    }
    #endregion

    #region 11. 仿真运行（新界面：生成结果文档）
    /// <summary>
    /// 运行仿真并创建结果文档
    /// </summary>
    [RelayCommand]
    private void RunSimulationAndCreateDocument()
    {
        // 执行原有仿真逻辑
        RunSimulation();

        // 创建仿真结果文档
        if (Layout != null && Factory != null)
        {
            try
            {
                var resultDoc = new SimulationResultDocumentViewModel(
                    $"仿真结果 {DateTime.Now:HH:mm:ss}",
                    SimulationResultText,
                    SimulationAsciiText
                );

                // 查找文档容器并添加结果文档
                var docDock = FindDocumentDock(Layout);
                if (docDock != null)
                {
                    Factory.AddDockable(docDock, resultDoc);
                    Factory.SetActiveDockable(resultDoc);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Create simulation document failed: {ex}");
            }
        }
    }

    /// <summary>
    /// 递归查找文档容器
    /// </summary>
    /// <param name="root">根布局节点</param>
    /// <returns>文档容器</returns>
    private DocumentDock? FindDocumentDock(IDockable root)
    {
        if (root is DocumentDock dd) return dd;

        if (root is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                var found = FindDocumentDock(child);
                if (found != null) return found;
            }
        }

        return null;
    }
    #endregion

    #region 12. 普通仿真（核心逻辑）
    /// <summary>
    /// 运行光学仿真核心逻辑
    /// </summary>
    [RelayCommand]
    private void RunSimulation()
    {
        // 链路为空时提示
        if (Elements.Count == 0)
        {
            SimulationResultText = "链路为空，无法仿真。";
            SimulationAsciiText = string.Empty;
            return;
        }

        // 转换链路为模型集合
        var models = Elements.Select(e => e.ToModel()).ToList();
        // 创建仿真器实例
        var simulator = new OpticalSimulator();

        // 仿真设置
        var settings = new OpticalSimulator.SimulationSettings
        {
            DefaultElementSpacing = 100.0,
            TreatMirrorAsThinLens = true
        };

        // 运行仿真
        var result = simulator.Run(models, settings);

        // 构建仿真结果文本
        var sb = new StringBuilder();
        sb.AppendLine(result.DetailedReport);

        // 添加ASCII光路描述
        if (!string.IsNullOrWhiteSpace(result.AsciiDescription))
        {
            sb.AppendLine();
            sb.AppendLine("ASCII 光路描述:");
            sb.AppendLine(result.AsciiDescription);
        }

        // 添加探测器结果
        if (result.DetectorState != null)
        {
            sb.AppendLine();
            sb.AppendLine("在探测器上的结果:");
            sb.AppendLine($"  z = {result.DetectorState.Z} mm");
            sb.AppendLine($"  w(z) = {result.DetectorState.BeamRadiusW} mm");
            sb.AppendLine($"  Power = {result.DetectorState.Power} W");
        }

        // 更新仿真结果文本（触发UI刷新）
        SimulationResultText = sb.ToString();
        SimulationAsciiText = result.AsciiDescription;

        // 处理探测器热力图数据
        try
        {
            for (int i = 0; i < models.Count && i < result.States.Count; i++)
            {
                var mdl = models[i];
                var state = result.States[i];

                // 仅处理探测器元件
                if (mdl.Type == OpticalElementType.Detector)
                {
                    if (i < Elements.Count)
                    {
                        var vm = Elements[i];

                        // 构建探测器结果文本
                        var detSb = new StringBuilder();
                        detSb.AppendLine($"探测器:  {vm.Name}");
                        detSb.AppendLine($"位置 z = {state.Z} mm");
                        detSb.AppendLine($"光斑半径 w(z) = {state.BeamRadiusW} mm");
                        detSb.AppendLine($"波前曲率 R(z) = {state.WavefrontRadiusR} mm");
                        detSb.AppendLine($"接收功率 = {state.Power} W");
                        detSb.AppendLine();
                        detSb.AppendLine("详细报告：");
                        detSb.AppendLine(state.Description ?? string.Empty);
                        vm.DetectorResultText = detSb.ToString();

                        // 计算热力图参数
                        var width = vm.DetectorWidth > 0 ? vm.DetectorWidth : 6.0;
                        var height = vm.DetectorHeight > 0 ? vm.DetectorHeight : 4.0;

                        int nx = 160;
                        int ny = 120;

                        double xMin = -width / 2.0, xMax = width / 2.0;
                        double yMin = -height / 2.0, yMax = height / 2.0;

                        // 初始化热力图数据矩阵
                        var map = new double[ny, nx];

                        var w = Math.Max(0.05, state.BeamRadiusW);
                        var p = Math.Max(0.0, state.Power);

                        double i0 = p;

                        // 填充热力图数据（高斯分布）
                        for (int yy = 0; yy < ny; yy++)
                        {
                            double y = yMin + (yMax - yMin) * yy / (ny - 1);
                            for (int xx = 0; xx < nx; xx++)
                            {
                                double x = xMin + (xMax - xMin) * xx / (nx - 1);
                                double r2 = x * x + y * y;
                                map[yy, xx] = i0 * Math.Exp(-2.0 * r2 / (w * w));
                            }
                        }

                        // 更新探测器VM的热力图数据
                        vm.HasDetectorMap = true;
                        vm.DetectorMap = map;
                        vm.DetectorMapXMin = xMin;
                        vm.DetectorMapXMax = xMax;
                        vm.DetectorMapYMin = yMin;
                        vm.DetectorMapYMax = yMax;
                    }
                }
            }
        }
        catch
        {
            // 静默处理热力图生成异常
        }
    }
    #endregion

    #region 13. 主题切换
    /// <summary>
    /// 切换应用主题（亮色/暗色）
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        try
        {
            ThemeManager.ToggleTheme();
        }
        catch { }
    }
    #endregion

    #region 14. 弃用代码（保留仅作兼容参考）
    /// <summary>
    /// 元件库搜索文本（弃用）
    /// </summary>
    private string _librarySearch = string.Empty;

    /// <summary>
    /// 筛选后的所有元件（弃用）
    /// </summary>
    public ObservableCollection<OpticalElement> FilteredAllElements { get; } = new();

    /// <summary>
    /// 随机数生成器（弃用）
    /// </summary>
    private static readonly Random RandomGenerator = new();

    /// <summary>
    /// 通知拖拽完成（调试用，弃用）
    /// </summary>
    [RelayCommand]
    private void NotifyMoveItemComplete(object? _)
    {
        Debug.WriteLine("MoveItem complete.  Current order:");
        foreach (var e in Elements)
            Debug.WriteLine($"- {e.Name}");
    }

    /// <summary>
    /// 选中的库项（弃用）
    /// </summary>
    [ObservableProperty]
    private OpticalElement? selectedLibraryItem;
    #endregion

    #region 15. 调试工具：打印链路信息
    /// <summary>
    /// 打印链路中所有元件的信息到调试输出窗口（调试用）
    /// </summary>
    [RelayCommand]
    void PrintElementsInfo()
    {
        Debug.WriteLine("--- 开始打印 Elements 集合信息 ---");

        if (Elements.Count == 0)
        {
            Debug.WriteLine("集合为空。");
        }
        else
        {
            foreach (var element in Elements)
            {
                string elementInfo = $"元素: Name='{element.Name}', ID='{element.Id}'";
                Debug.WriteLine(elementInfo);
            }
        }

        Debug.WriteLine("--- 打印完成 ---");
    }
    #endregion 
}
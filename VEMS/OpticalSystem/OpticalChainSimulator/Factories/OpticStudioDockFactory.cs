using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using OpticalChainSimulator.ViewModels;
using OpticalChainSimulator.ViewModels.Docks;
using System;
using System.Collections.Generic;

namespace OpticalChainSimulator.Factories
{
    /// <summary>
    /// 光学仿真工作室的Dock布局工厂类
    /// 继承自Dock.Model.Mvvm的Factory基类，负责创建和初始化应用的整体Dock布局结构
    /// 定义了左侧元件库、中央3D视口、右侧工程/仿真面板的布局组合规则
    /// </summary>
    public class OpticStudioDockFactory : Factory
    {
        /// <summary>
        /// 主窗口视图模型实例，作为所有Dockable组件的DataContext来源
        /// </summary>
        private readonly MainWindowViewModel _context;

        /// <summary>
        /// 初始化OpticStudioDockFactory实例
        /// </summary>
        /// <param name="context">主窗口视图模型，不能为空</param>
        /// <exception cref="ArgumentNullException">当传入的context为null时抛出该异常</exception>
        public OpticStudioDockFactory(MainWindowViewModel context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 重写基类方法，创建应用的完整Dock布局结构
        /// 按左侧工具面板、中央3D视口文档区、右侧工具面板的结构组合成主布局，并封装为根布局
        /// </summary>
        /// <returns>构建完成的根Dock布局节点（IRootDock）</returns>
        public override IRootDock CreateLayout()
        {
            // ✅ 构建左侧工具面板（元件库）
            var libraryTool = new LibraryToolViewModel();

            var leftTools = new ToolDock
            {
                Id = "LeftTools",               // 左侧工具面板唯一标识
                Title = "左侧工具",              // 面板显示标题
                Proportion = 0.2,               // 占整体布局宽度的20%
                ActiveDockable = libraryTool,   // 默认激活元件库面板
                VisibleDockables = CreateList<IDockable>(libraryTool), // 面板内可见的Dockable列表
                Alignment = Alignment.Left      // 停靠在布局左侧
            };

            // ✅ 构建中央文档区（3D视口）
            var viewportDoc = new ViewportDocumentViewModel();

            var documentsPane = new DocumentDock
            {
                Id = "DocumentsPane",           // 中央文档区唯一标识
                Title = "文档",                 // 面板显示标题
                IsCollapsable = false,          // 禁止折叠中央文档区
                ActiveDockable = viewportDoc,   // 默认激活3D视口
                VisibleDockables = CreateList<IDockable>(viewportDoc), // 仅包含3D视口组件
                CanCreateDocument = false       // 禁止创建新的文档面板
            };

            // ✅ 构建右侧工具面板（工程信息+仿真结果）
            var projectTool = new ProjectToolViewModel();     // 工程信息面板ViewModel
            var simulationTool = new SimulationToolViewModel(); // 仿真结果面板ViewModel

            var rightTools = new ToolDock
            {
                Id = "RightTools",              // 右侧工具面板唯一标识
                Title = "右侧工具",              // 面板显示标题
                Proportion = 0.2,               // 占整体布局宽度的20%
                ActiveDockable = projectTool,   // 默认激活工程信息面板
                VisibleDockables = CreateList<IDockable>(projectTool, simulationTool), // 包含工程和仿真两个面板
                Alignment = Alignment.Right     // 停靠在布局右侧
            };

            // ✅ 构建主布局（水平排列左/中/右面板）
            var mainLayout = new ProportionalDock
            {
                Id = "MainLayout",              // 主布局唯一标识
                Title = "MainLayout",           // 布局显示标题
                Proportion = double.NaN,        // 不设置固定比例，由子元素比例决定
                Orientation = Orientation.Horizontal, // 子元素水平排列
                ActiveDockable = null,          // 主布局无默认激活的子元素
                // 子元素包含：左侧面板、分割器、中央文档区、分割器、右侧面板
                VisibleDockables = CreateList<IDockable>(
                    leftTools,
                    new ProportionalDockSplitter(), // 左侧面板与中央文档区间的分割器
                    documentsPane,
                    new ProportionalDockSplitter(), // 中央文档区与右侧面板间的分割器
                    rightTools
                )
            };

            // ✅ 构建根Dock（应用最顶层布局容器）
            var rootDock = CreateRootDock();
            rootDock.Id = "Root";              // 根布局唯一标识
            rootDock.Title = "Root";           // 布局显示标题
            rootDock.IsCollapsable = false;    // 禁止折叠根布局
            rootDock.ActiveDockable = mainLayout; // 默认激活主布局
            rootDock.DefaultDockable = mainLayout; // 默认显示的布局为主体布局
            rootDock.VisibleDockables = CreateList<IDockable>(mainLayout); // 仅包含主布局

            return rootDock;
        }

        /// <summary>
        /// 重写基类方法，初始化Dock布局的上下文和定位器
        /// 核心配置：为所有Dockable节点绑定主窗口ViewModel作为DataContext，配置宿主窗口和Dockable定位器
        /// </summary>
        /// <param name="layout">需要初始化的Dock布局根节点</param>
        public override void InitLayout(IDockable layout)
        {
            // ✅ 配置上下文定位器：为每个Dockable节点指定DataContext（均指向主窗口ViewModel）
            ContextLocator = new Dictionary<string, Func<object?>>
            {
                ["Root"] = () => _context,
                ["MainLayout"] = () => _context,
                ["LeftTools"] = () => _context,
                ["LibraryTool"] = () => _context,
                ["DocumentsPane"] = () => _context,
                ["ViewportDocument"] = () => _context,
                ["RightTools"] = () => _context,
                ["ProjectTool"] = () => _context,
                ["SimulationTool"] = () => _context,
            };

            // ✅ 配置宿主窗口定位器：指定IDockWindow对应的宿主窗口实现
            HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () => new HostWindow()
            };

            // ✅ 配置Dockable定位器：当前无自定义Dockable定位逻辑，留空
            DockableLocator = new Dictionary<string, Func<IDockable?>> { };

            // 调用基类初始化方法完成布局的最终初始化
            base.InitLayout(layout);
        }
    }
}
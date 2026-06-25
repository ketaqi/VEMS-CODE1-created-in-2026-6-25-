using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using System;
using System.Linq;

namespace OpticalChainSimulator.Factories;

/// <summary>
/// Dock布局预设工具类
/// 提供编辑、分析、仿真三种操作模式下的布局比例配置，通过调整各面板占比适配不同使用场景
/// </summary>
public static class LayoutPresets
{
    /// <summary>
    /// 应用编辑模式布局（强调元件库和属性编辑）
    /// 调整布局比例：加宽左侧工具面板，适配元件选择、属性编辑等核心操作
    /// </summary>
    /// <param name="factory">光学工作室Dock布局工厂实例</param>
    /// <param name="layout">需要调整的Dock布局根节点</param>
    public static void ApplyEditMode(OpticStudioDockFactory factory, IDock layout)
    {
        // 查找主布局节点（MainLayout），未找到则直接返回
        var mainLayout = FindMainLayout(layout);
        if (mainLayout == null) return;

        // 从主布局中获取左、中、右面板节点
        var left = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "LeftTools");
        var center = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "Documents");
        var right = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "RightTools");

        // 调整编辑模式比例：左侧30%、中央50%、右侧20%
        if (left != null) left.Proportion = 0.3;
        if (center != null) center.Proportion = 0.5;
        if (right != null) right.Proportion = 0.2;
    }

    /// <summary>
    /// 应用分析模式布局（强调3D视口和仿真结果）
    /// 调整布局比例：加宽右侧工具面板，适配仿真结果查看、数据分析等操作
    /// </summary>
    /// <param name="factory">光学工作室Dock布局工厂实例</param>
    /// <param name="layout">需要调整的Dock布局根节点</param>
    public static void ApplyAnalysisMode(OpticStudioDockFactory factory, IDock layout)
    {
        // 查找主布局节点（MainLayout），未找到则直接返回
        var mainLayout = FindMainLayout(layout);
        if (mainLayout == null) return;

        // 从主布局中获取左、中、右面板节点
        var left = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "LeftTools");
        var center = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "Documents");
        var right = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "RightTools");

        // 调整分析模式比例：左侧15%、中央50%、右侧35%
        if (left != null) left.Proportion = 0.15;
        if (center != null) center.Proportion = 0.5;
        if (right != null) right.Proportion = 0.35;
    }

    /// <summary>
    /// 应用仿真模式布局（最大化3D视口）
    /// 调整布局比例：最大化中央3D视口面板，适配仿真过程可视化、交互操作
    /// </summary>
    /// <param name="factory">光学工作室Dock布局工厂实例</param>
    /// <param name="layout">需要调整的Dock布局根节点</param>
    public static void ApplySimulationMode(OpticStudioDockFactory factory, IDock layout)
    {
        // 查找主布局节点（MainLayout），未找到则直接返回
        var mainLayout = FindMainLayout(layout);
        if (mainLayout == null) return;

        // 从主布局中获取左、中、右面板节点
        var left = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "LeftTools");
        var center = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "Documents");
        var right = mainLayout.VisibleDockables?.FirstOrDefault(x => x.Id == "RightTools");

        // 调整仿真模式比例：左侧10%、中央70%、右侧20%
        if (left != null) left.Proportion = 0.1;
        if (center != null) center.Proportion = 0.7;
        if (right != null) right.Proportion = 0.2;
    }

    /// <summary>
    /// 私有辅助方法：递归查找布局树中的MainLayout节点（ProportionalDock类型）
    /// </summary>
    /// <param name="root">待查找的布局节点（起始为根节点）</param>
    /// <returns>找到的MainLayout节点（ProportionalDock）；未找到返回null</returns>
    private static ProportionalDock? FindMainLayout(IDockable? root)
    {
        // 直接匹配：当前节点是ProportionalDock且ID为MainLayout，返回该节点
        if (root is ProportionalDock pd && pd.Id == "MainLayout")
            return pd;

        // 递归查找：当前节点是IDock类型，遍历其子节点继续查找
        if (root is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Enumerable.Empty<IDockable>())
            {
                var found = FindMainLayout(child);
                if (found != null) return found;
            }
        }

        // 未找到匹配节点，返回null
        return null;
    }
}
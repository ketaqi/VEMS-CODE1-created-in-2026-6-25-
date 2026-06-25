using Dock.Model.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace OpticalChainSimulator.Services;

/// <summary>
/// Dock 布局序列化器（简化版）
/// 负责将 Dock 布局的关键状态（比例、激活项）序列化为 JSON 并保存到文件，
/// 以及从 JSON 文件反序列化并恢复布局状态
/// </summary>
public static class DockLayoutSerializer
{
    /// <summary>
    /// 将 Dock 布局的关键状态保存到指定路径的 JSON 文件
    /// </summary>
    /// <param name="path">保存布局文件的完整路径（包含文件名和扩展名）</param>
    /// <param name="layout">需要保存状态的 Dock 布局根对象</param>
    public static void SaveLayout(string path, IDock layout)
    {
        // 初始化布局状态容器
        var state = new DockLayoutState();
        // 递归收集布局中所有 Dockable 的关键状态
        CollectState(layout, state);

        // 将布局状态序列化为格式化的 JSON 字符串
        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
        {
            WriteIndented = true // 启用 JSON 格式化，提升可读性
        });

        // 将 JSON 字符串写入指定文件
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// 从指定路径的 JSON 文件加载布局状态并应用到 Dock 布局
    /// </summary>
    /// <param name="path">布局文件的完整路径（包含文件名和扩展名）</param>
    /// <param name="layout">需要恢复状态的 Dock 布局根对象</param>
    public static void LoadLayout(string path, IDock layout)
    {
        // 检查文件是否存在，不存在则直接返回
        if (!File.Exists(path)) return;

        // 读取文件中的 JSON 字符串
        var json = File.ReadAllText(path);
        // 将 JSON 字符串反序列化为布局状态对象
        var state = JsonSerializer.Deserialize<DockLayoutState>(json);

        // 状态对象不为空时，递归应用状态到布局
        if (state != null)
        {
            ApplyState(layout, state);
        }
    }

    /// <summary>
    /// 递归遍历 Dockable 树，收集布局的关键状态（比例、激活项）
    /// </summary>
    /// <param name="dockable">当前遍历的 Dockable 节点</param>
    /// <param name="state">用于存储收集到的布局状态的容器</param>
    private static void CollectState(IDockable dockable, DockLayoutState state)
    {
        // 仅处理有有效 ID 的 Dockable 节点
        if (!string.IsNullOrEmpty(dockable.Id))
        {
            // 保存当前节点的比例值
            state.Proportions[dockable.Id] = dockable.Proportion;

            // 如果当前节点是 Dock 类型且存在激活的子项，保存激活项的 ID
            if (dockable is IDock dock && dock.ActiveDockable != null)
            {
                state.ActiveDockables[dockable.Id] = dock.ActiveDockable.Id ?? "";
            }
        }

        // 如果当前节点是 Dock 类型，递归处理其所有可见子项
        if (dockable is IDock d)
        {
            foreach (var child in d.VisibleDockables ?? Array.Empty<IDockable>())
            {
                CollectState(child, state);
            }
        }
    }

    /// <summary>
    /// 递归遍历 Dockable 树，将保存的布局状态应用到对应节点
    /// </summary>
    /// <param name="dockable">当前遍历的 Dockable 节点</param>
    /// <param name="state">包含待恢复布局状态的容器</param>
    private static void ApplyState(IDockable dockable, DockLayoutState state)
    {
        // 仅处理有有效 ID 且存在对应比例配置的 Dockable 节点
        if (!string.IsNullOrEmpty(dockable.Id))
        {
            if (state.Proportions.ContainsKey(dockable.Id))
            {
                // 恢复节点的比例值
                dockable.Proportion = state.Proportions[dockable.Id];
            }
        }

        // 如果当前节点是 Dock 类型，递归处理其所有可见子项
        if (dockable is IDock dock)
        {
            foreach (var child in dock.VisibleDockables ?? Array.Empty<IDockable>())
            {
                ApplyState(child, state);
            }
        }
    }

    /// <summary>
    /// 布局状态数据容器
    /// 用于序列化/反序列化的中间对象，存储布局的关键状态
    /// </summary>
    private class DockLayoutState
    {
        /// <summary>
        /// 键：Dockable 节点 ID，值：节点的比例值
        /// </summary>
        public Dictionary<string, double> Proportions { get; set; } = new();

        /// <summary>
        /// 键：Dock 节点 ID，值：该节点当前激活子项的 ID
        /// </summary>
        public Dictionary<string, string> ActiveDockables { get; set; } = new();
    }
}
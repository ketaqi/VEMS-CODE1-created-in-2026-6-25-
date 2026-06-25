using System;
using System.Collections.Generic;
using System.Text.Json;
using OpticalChainSimulator.Models;

namespace OpticalChainSimulator.Services.ComponentServices;

/// <summary>
/// 光学元件插件通用接口
/// 定义光学元件插件的核心契约，插件需实现此接口以接入主程序的元件库体系
/// 插件至少需返回一个或多个模板模型用于元件库展示，可选实现自定义ViewModel/编辑器或元数据扩展
/// </summary>
public interface IOpticalElementPlugin
{
    /// <summary>
    /// 插件唯一标识
    /// 采用反向域名格式（例如 "com.mycompany.beamsplitter"），确保全局唯一，避免冲突
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 插件显示名称
    /// 用于UI界面展示（如元件库分类、插件列表），支持友好的中文名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件版本号
    /// 遵循语义化版本规范（SemVer），格式为 主版本.次版本.修订号（例如 1.0.0）
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 获取插件提供的光学元件模板（默认实例）
    /// 这些模板实例将被添加到主程序的元件库中，供用户选择和使用
    /// 每个返回的OpticalElement对象需在其Properties属性中标注PluginId/PluginType（可由插件加载器自动注入）
    /// </summary>
    /// <returns>光学元件模板集合，不能为空（至少包含一个模板）</returns>
    IEnumerable<OpticalElement> GetTemplates();

    /// <summary>
    /// 可选：为已有光学元件模型构建自定义ViewModel
    /// 插件可通过此方法提供自定义的视图模型/编辑器逻辑，若返回null则主程序使用通用的OpticalElementViewModel
    /// </summary>
    /// <param name="model">待绑定的光学元件模型实例</param>
    /// <returns>自定义的光学元件ViewModel实例，或null（使用通用实现）</returns>
    OpticalChainSimulator.ViewModels.OpticalElementViewModel? CreateViewModel(OpticalElement model);

    /// <summary>
    /// 插件暴露的扩展元数据
    /// 用于UI展示或自动表单生成，包含参数描述、图标资源路径等信息
    /// 采用任意JSON结构，主程序按约定解析，可为null（无扩展元数据）
    /// </summary>
    JsonElement? Metadata { get; }
}
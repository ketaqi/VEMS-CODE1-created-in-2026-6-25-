using OpticalChainSimulator.yuanjianku;

namespace OpticalChainSimulator.Models;

/// <summary>
/// 光学仿真项目实体类
/// 封装光学仿真项目的核心数据，包含项目基本信息和两类光学元件集合
/// </summary>
public class OpticalProject
{
    /// <summary>
    /// 项目名称
    /// 默认值为 "Untitled Project"（未命名项目）
    /// </summary>
    public string Name { get; set; } = "Untitled Project";

    /// <summary>
    /// 项目描述信息
    /// 默认值为空字符串
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// OpticalElement 类型的光学元件集合
    /// 默认初始化为空列表
    /// </summary>
    public List<OpticalElement> Elements { get; set; } = new();

    /// <summary>
    /// OpticalElement1 类型的光学元件集合
    /// 默认初始化为空列表
    /// </summary>
    public List<OpticalElement1> Elements1 { get; set; } = new();
}
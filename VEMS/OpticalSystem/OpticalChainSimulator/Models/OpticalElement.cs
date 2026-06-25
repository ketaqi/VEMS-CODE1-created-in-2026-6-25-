namespace OpticalChainSimulator.Models;

using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// 光学元件核心模型类
/// 封装光学元件的基础标识、类型、名称，以及数值参数和扩展属性，是仿真系统中所有光学元件的基础数据载体
/// </summary>
public class OpticalElement
{
    /// <summary>
    /// 光学元件唯一标识
    /// 默认为自动生成的新GUID，确保每个元件实例全局唯一，用于场景中元件的定位和管理
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 光学元件类型
    /// 关联OpticalElementType枚举，标识元件的具体品类（如光源、反射镜、透镜等）
    /// </summary>
    public OpticalElementType Type { get; set; }

    /// <summary>
    /// 光学元件名称
    /// 用于UI展示、日志输出和用户标识，默认值为空字符串
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 数值参数字典
    /// 存储元件的纯数值类型参数（键为参数名，值为double类型数值），兼容现有代码和模拟器的使用习惯
    /// 默认初始化为空字典
    /// </summary>
    public Dictionary<string, double> Parameters { get; set; } = new();

    /// <summary>
    /// 扩展属性字典（任意JSON类型）
    /// 存储非纯数值的任意JSON字段（字符串、数字、对象等），主要用于导入外部库文件时保留非标字段
    /// 序列化/反序列化时会保留字段的原始JSON结构，默认初始化为空字典
    /// </summary>
    public Dictionary<string, JsonElement> Properties { get; set; } = new();

    /// <summary>
    /// 重写ToString方法，返回元件的类型和名称组合字符串
    /// 便于调试、日志输出和UI快速显示元件核心信息
    /// </summary>
    /// <returns>格式为“元件类型 - 元件名称”的字符串（例：Source - 激光光源）</returns>
    public override string ToString()
    {
        return $"{Type} - {Name}";
    }
}
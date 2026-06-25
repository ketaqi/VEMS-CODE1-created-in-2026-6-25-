namespace OpticalChainSimulator.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpticalChainSimulator.Models;

/// <summary>
/// 光学元件库导入器
/// 读取符合 library_sample.json 格式的JSON文件，将文件中的元件条目转换为OpticalElement对象列表
/// 自动区分数值类型参数（存入Parameters字典）和非数值类型属性（存入Properties字典），兼容现有仿真系统的数据结构
/// </summary>
public static class LibraryImporter
{
    /// <summary>
    /// 从指定路径加载光学元件库JSON文件，并转换为OpticalElement列表
    /// </summary>
    /// <param name="filePath">元件库JSON文件的完整路径（包含文件名和扩展名）</param>
    /// <returns>转换后的OpticalElement对象列表；文件不存在/格式不合法时返回空列表</returns>
    public static List<OpticalElement> LoadLibrary(string filePath)
    {
        // 初始化返回结果列表
        var result = new List<OpticalElement>();

        // 检查文件是否存在，不存在直接返回空列表
        if (!File.Exists(filePath))
            return result;

        // 读取JSON文件内容
        var json = File.ReadAllText(filePath);
        // 解析JSON内容为JsonDocument（使用using确保资源释放）
        using var doc = JsonDocument.Parse(json);

        // 获取JSON根节点
        var root = doc.RootElement;

        // 尝试获取根节点下的Items数组属性，不存在则返回空列表
        if (!root.TryGetProperty("Items", out var itemsElement))
            return result;

        // 遍历Items数组中的每个元件条目
        foreach (var item in itemsElement.EnumerateArray())
        {
            // 创建新的光学元件实例
            var el = new OpticalElement();

            // 解析并设置元件名称（仅处理字符串类型的Name字段）
            if (item.TryGetProperty("Name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                el.Name = nameProp.GetString() ?? string.Empty;

            // 解析并设置元件类型（字符串转OpticalElementType枚举）
            if (item.TryGetProperty("Type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
            {
                var typeStr = typeProp.GetString() ?? string.Empty;
                // 尝试忽略大小写解析枚举类型
                if (Enum.TryParse<OpticalElementType>(typeStr, ignoreCase: true, out var t))
                {
                    el.Type = t;
                }
                else
                {
                    // 未知类型处理：默认设为Source，并将原始Type字段存入Properties保留信息
                    el.Type = OpticalElementType.Source;
                    el.Properties["Type"] = typeProp.Clone();
                }
            }

            // 遍历条目所有字段，区分数值/非数值类型填充对应字典（跳过已处理的Name/Type字段）
            foreach (var p in item.EnumerateObject())
            {
                var key = p.Name;
                // 跳过已单独处理的Name和Type字段
                if (key.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("Type", StringComparison.OrdinalIgnoreCase))
                    continue;

                // 克隆JSON元素（避免JsonDocument释放后引用失效）
                var val = p.Value.Clone();
                // 数值类型字段存入Parameters字典
                if (val.ValueKind == JsonValueKind.Number && val.TryGetDouble(out var d))
                {
                    el.Parameters[key] = d;
                }
                // 非数值类型字段存入Properties字典
                else
                {
                    el.Properties[key] = val;
                }
            }

            // 将解析后的元件添加到结果列表
            result.Add(el);
        }

        return result;
    }
}
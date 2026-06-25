using System.IO;
using System.Text.Json;

namespace OpticalChainSimulator.Services;

/// <summary>
/// 光学项目序列化服务类
/// 提供OpticalProject对象的JSON序列化（保存）和反序列化（加载）功能
/// </summary>
public static class ProjectSerializer
{
    /// <summary>
    /// JSON序列化配置选项
    /// - 启用缩进格式化输出
    /// - 属性名采用驼峰命名法（CamelCase）
    /// </summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 将光学项目对象保存为JSON文件
    /// </summary>
    /// <param name="filePath">保存文件的完整路径（包含文件名和扩展名）</param>
    /// <param name="project">要序列化保存的OpticalProject对象</param>
    public static void Save(string filePath, OpticalProject project)
    {
        // 将项目对象序列化为JSON字符串
        var json = JsonSerializer.Serialize(project, Options);
        // 将JSON字符串写入指定文件（覆盖已有文件）
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 从JSON文件加载并反序列化为光学项目对象
    /// </summary>
    /// <param name="filePath">要加载的JSON文件完整路径</param>
    /// <returns>
    /// 成功加载返回OpticalProject对象；
    /// 文件不存在返回null；
    /// 反序列化失败会抛出对应异常（如JsonException）
    /// </returns>
    public static OpticalProject? Load(string filePath)
    {
        // 检查文件是否存在，不存在直接返回null
        if (!File.Exists(filePath))
            return null;

        // 读取文件中的JSON字符串
        var json = File.ReadAllText(filePath);
        // 将JSON字符串反序列化为OpticalProject对象
        return JsonSerializer.Deserialize<OpticalProject>(json, Options);
    }
}


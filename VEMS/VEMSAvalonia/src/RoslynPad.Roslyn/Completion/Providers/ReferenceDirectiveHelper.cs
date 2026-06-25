/// <summary>
/// 引用指令辅助类，提供NuGet引用解析等功能
/// </summary>
internal static class ReferenceDirectiveHelper
{
    /// <summary>
    /// NuGet引用前缀（nuget:）
    /// </summary>
    public const string NuGetPrefix = "nuget:";

    /// <summary>
    /// NuGet引用分隔符（/ ,）
    /// </summary>
    private static readonly char[] s_nugetSeparators = ['/', ','];

    /// <summary>
    /// 解析NuGet引用字符串，提取包ID和版本
    /// </summary>
    /// <param name="value">NuGet引用字符串（格式：nuget:包ID[,/版本]）</param>
    /// <returns>元组（包ID，版本），版本可为null</returns>
    public static (string id, string? version) ParseNuGetReference(string value)
    {
        string id;
        string? version;

        // 查找分隔符位置
        var indexOfSlash = value.IndexOfAny(s_nugetSeparators);
        if (indexOfSlash >= 0)
        {
            // 提取包ID（去掉前缀）
            id = value.Substring(NuGetPrefix.Length, indexOfSlash - NuGetPrefix.Length);
            // 提取版本（分隔符后内容，若分隔符在末尾则版本为空字符串）
            version = indexOfSlash != value.Length - 1 ? value.Substring(indexOfSlash + 1) : string.Empty;
        }
        else
        {
            // 无分隔符时仅提取包ID，版本为null
            id = value.Substring(NuGetPrefix.Length);
            version = null;
        }

        // 去除首尾空白后返回
        return (id.Trim(), version?.Trim());
    }
}

using System.Diagnostics;

namespace RoslynPad.Runtime;

/// <summary>
/// 提供 <see cref="Process"/> 的扩展方法。
/// </summary>
internal static class ProcessExtensions
{
    /// <summary>
    /// 检查进程是否仍在运行。
    /// </summary>
    /// <param name="process">要检查的进程。</param>
    /// <returns>如果进程仍在运行则返回 true，否则返回 false。</returns>
    public static bool IsAlive(this Process process)
    {
        try
        {
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }
}

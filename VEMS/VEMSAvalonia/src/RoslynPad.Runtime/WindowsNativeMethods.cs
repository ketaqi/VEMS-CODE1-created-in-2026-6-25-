using System.Runtime.InteropServices;

namespace RoslynPad.Runtime;

/// <summary>
/// 提供 Windows 平台的本地方法调用。
/// </summary>
internal static partial class WindowsNativeMethods
{
    /// <summary>
    /// 禁用 Windows 错误报告（WER），使进程在发生错误时快速失败而不显示错误对话框。
    /// </summary>
    /// <remarks>
    /// 此方法仅在 Windows 7 (6.1) 及更高版本上有效。
    /// 设置的错误模式包括：
    /// <list type="bullet">
    /// <item><description>SEM_FAILCRITICALERRORS - 不显示严重错误对话框</description></item>
    /// <item><description>SEM_NOOPENFILEERRORBOX - 不显示文件打开错误对话框</description></item>
    /// <item><description>SEM_NOGPFAULTERRORBOX - 不显示 GP 错误对话框</description></item>
    /// </list>
    /// </remarks>
    internal static void DisableWer()
    {
        if (Environment.OSVersion.Version < new Version(6, 1, 0, 0))
        {
            return;
        }

        SetErrorMode(GetErrorMode() |
            ErrorMode.SEM_FAILCRITICALERRORS |
            ErrorMode.SEM_NOOPENFILEERRORBOX |
            ErrorMode.SEM_NOGPFAULTERRORBOX);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// 设置进程的错误模式。
    /// </summary>
    /// <param name="mode">要设置的错误模式。</param>
    /// <returns>先前的错误模式。</returns>
    [LibraryImport("kernel32")]
    private static partial
#else
    /// <summary>
    /// 设置进程的错误模式。
    /// </summary>
    /// <param name="mode">要设置的错误模式。</param>
    /// <returns>先前的错误模式。</returns>
    [DllImport("kernel32", PreserveSig = true)]
    private static extern 
#endif
    ErrorMode SetErrorMode(ErrorMode mode);

#if NET8_0_OR_GREATER
    /// <summary>
    /// 获取当前进程的错误模式。
    /// </summary>
    /// <returns>当前的错误模式。</returns>
    [LibraryImport("kernel32")]
    private static partial
#else
    /// <summary>
    /// 获取当前进程的错误模式。
    /// </summary>
    /// <returns>当前的错误模式。</returns>
    [DllImport("kernel32", PreserveSig = true)]
    private static extern 
#endif
    ErrorMode GetErrorMode();

    /// <summary>
    /// Windows 错误模式标志。
    /// </summary>
    [Flags]
    private enum ErrorMode
    {
        /// <summary>
        /// 系统不显示严重错误处理程序消息框。
        /// </summary>
        SEM_FAILCRITICALERRORS = 0x0001,

        /// <summary>
        /// 系统不显示 Windows 错误报告对话框。
        /// </summary>
        SEM_NOGPFAULTERRORBOX = 0x0002,

        /// <summary>
        /// 系统自动修复内存对齐错误并使其对应用程序不可见。
        /// </summary>
        SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,

        /// <summary>
        /// 当找不到文件时，系统不显示消息框。
        /// </summary>
        SEM_NOOPENFILEERRORBOX = 0x8000,
    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace RoslynPad.Runtime;

/// <summary>
/// RoslynPad 独立宿主的运行时初始化器。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeInitializer
{
    private static bool s_initialized;

    /// <summary>
    /// 初始化运行时环境。
    /// </summary>
    /// <remarks>
    /// 此方法会：
    /// <list type="bullet">
    /// <item><description>禁用 Windows 错误报告</description></item>
    /// <item><description>附加到父进程（用于进程生命周期管理）</description></item>
    /// <item><description>设置控制台重定向</description></item>
    /// </list>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Initialize()
    {
        if (s_initialized)
        {
            return;
        }

        s_initialized = true;

        DisableWer();

        var useJson = UseJsonOutput() || TryAttachToParentProcess();
        AttachConsole(useJson);
    }

    /// <summary>
    /// 附加控制台重定向。
    /// </summary>
    private static void AttachConsole(bool useJson)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var consoleDumper = useJson ? (IConsoleDumper)new JsonConsoleDumper() : new DirectConsoleDumper();

        if (consoleDumper.SupportsRedirect)
        {
            Console.SetOut(consoleDumper.CreateWriter());
            Console.SetError(consoleDumper.CreateWriter("Error"));
            Console.SetIn(consoleDumper.CreateReader());

            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                consoleDumper.DumpException((Exception)e.ExceptionObject);
                Environment.Exit(1);
            };

            Helpers.Progress += progress => consoleDumper.DumpProgress(ProgressResultObject.Create(progress));
        }

        ObjectExtensions.Dumped += consoleDumper.Dump;
        AppDomain.CurrentDomain.ProcessExit += (o, e) => consoleDumper.Flush();
    }

#pragma warning disable CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'
    private static bool UseJsonOutput() => Environment.CommandLine.IndexOf("--json", StringComparison.OrdinalIgnoreCase) >= 0;
#pragma warning restore CA2249 // Consider using 'string.Contains' instead of 'string.IndexOf'

    /// <summary>
    /// 尝试附加到父进程。
    /// </summary>
    private static bool TryAttachToParentProcess()
    {
        if (!ParseCommandLine("pid", @"\d+", out var parentProcessId))
        {
            return false;
        }

        AttachToParentProcess(int.Parse(parentProcessId, CultureInfo.InvariantCulture));

        return true;
    }

    /// <summary>
    /// 附加到指定的父进程，当父进程退出时自动退出。
    /// </summary>
    /// <param name="parentProcessId">父进程 ID。</param>
    internal static void AttachToParentProcess(int parentProcessId)
    {
        Process clientProcess;
        try
        {
            clientProcess = Process.GetProcessById(parentProcessId);
        }
        catch (ArgumentException)
        {
            Environment.Exit(1);
            return;
        }

        clientProcess.EnableRaisingEvents = true;
        clientProcess.Exited += (_, _) => Environment.Exit(1);

        if (!clientProcess.IsAlive())
        {
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// 禁用 Windows 错误报告，使进程快速失败。
    /// </summary>
    internal static void DisableWer()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        WindowsNativeMethods.DisableWer();
    }

    /// <summary>
    /// 解析命令行参数。
    /// </summary>
    private static bool ParseCommandLine(string name, string pattern, out string value)
    {
        var match = Regex.Match(Environment.CommandLine, @$"--{name}\s+""?({pattern})");
        value = match.Success ? match.Groups[1].Value : string.Empty;
        return match.Success;
    }
}

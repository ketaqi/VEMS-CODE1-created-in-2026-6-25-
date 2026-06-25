using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RoslynPad.UI;

/// <summary>
/// 进程作业接口：定义进程组管理的跨平台抽象。
/// </summary>
/// <remarks>
/// <para>
/// 此接口用于将子进程添加到作业对象中，以便在父进程退出时自动终止整个进程树。
/// </para>
/// <para>
/// 平台支持：
/// <list type="bullet">
///   <item><description>Windows：使用 Job Object API 实现完整的进程树管理</description></item>
///   <item><description>其他平台：返回空操作实现（进程管理需要其他机制）</description></item>
/// </list>
/// </para>
/// </remarks>
internal interface IProcessJob : IDisposable
{
    /// <summary>
    /// 尝试将指定进程添加到作业对象。
    /// </summary>
    /// <param name="pid">进程 ID。</param>
    /// <returns>
    /// 如果成功将进程添加到作业对象返回 <c>true</c>；
    /// 如果失败或平台不支持返回 <c>false</c>。
    /// </returns>
    bool TryAddProcess(int pid);
}

/// <summary>
/// 进程作业工厂：创建适合当前平台的进程作业实现。
/// </summary>
/// <remarks>
/// <para>
/// 此类提供跨平台的进程组管理能力。在 Windows 上使用 Job Object API，
/// 可以确保当父进程退出时，所有子进程（包括子进程的子进程）都会被自动终止。
/// </para>
/// <para>
/// 这对于脚本执行宿主非常重要，可以防止孤儿进程的产生。
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var job = ProcessJob.Create();
/// 
/// // 启动子进程
/// var process = Process.Start(startInfo);
/// 
/// // 将子进程添加到作业
/// job.TryAddProcess(process.Id);
/// 
/// // 当 job 被 Dispose 时，所有关联的进程都会被终止
/// </code>
/// </example>
internal static class ProcessJob
{
    /// <summary>
    /// 创建适合当前平台的进程作业实现。
    /// </summary>
    /// <returns>
    /// Windows 平台返回 <see cref="WindowsJobObject"/>；
    /// 其他平台返回空操作实现。
    /// </returns>
    public static IProcessJob Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsJobObject();
        return new NoopJob();
    }

    /// <summary>
    /// 空操作作业实现：用于不支持作业对象的平台。
    /// </summary>
    private sealed class NoopJob : IProcessJob
    {
        /// <inheritdoc/>
        public void Dispose() { }

        /// <inheritdoc/>
        public bool TryAddProcess(int pid) => false;
    }

    /// <summary>
    /// Windows 作业对象实现：使用 Win32 Job Object API 管理进程组。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此实现创建一个配置了 <c>JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE</c> 标志的作业对象，
    /// 确保当作业句柄关闭时，所有关联的进程都会被终止。
    /// </para>
    /// </remarks>
    private sealed class WindowsJobObject : IProcessJob
    {
        private IntPtr _hJob;

        /// <summary>
        /// 初始化 Windows 作业对象。
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception">创建作业对象失败时抛出。</exception>
        public WindowsJobObject()
        {
            _hJob = CreateJobObject(IntPtr.Zero, null);
            if (_hJob == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                }
            };

            int length = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            IntPtr ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(info, ptr, false);
                if (!SetInformationJobObject(_hJob, JobObjectInfoClass.ExtendedLimitInformation, ptr, (uint)length))
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <inheritdoc/>
        public bool TryAddProcess(int pid)
        {
            try
            {
                using var proc = Process.GetProcessById(pid);
                return AssignProcessToJobObject(_hJob, proc.Handle);
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var h = _hJob;
            _hJob = IntPtr.Zero;
            if (h != IntPtr.Zero) CloseHandle(h);
        }

        #region Win32 常量和结构体

        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;

        private enum JobObjectInfoClass
        {
            BasicLimitInformation = 2,
            ExtendedLimitInformation = 9
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LARGE_INTEGER { public long QuadPart; }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
            public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public LARGE_INTEGER PerProcessUserTimeLimit, PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize, MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass, SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
        }

        #endregion

        #region Win32 P/Invoke

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? name);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoClass infoClass, IntPtr lpJobObjectInfo, uint cb);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }
}

using System.Diagnostics;
using System.Text;

namespace RoslynPad.Build;

/// <summary>
/// 提供进程管理工具方法。
/// </summary>
internal class ProcessUtil
{
    /// <summary>
    /// 异步运行进程并返回结果对象。
    /// </summary>
    /// <param name="path">可执行文件路径。</param>
    /// <param name="workingDirectory">工作目录。</param>
    /// <param name="arguments">命令行参数。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>进程结果对象。</returns>
    public static async Task<ProcessResult> RunProcessAsync(string path, string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = workingDirectory,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            },
            EnableRaisingEvents = true,
        };

        var exitTcs = new TaskCompletionSource<object?>();
        process.Exited += (_, _) => exitTcs.TrySetResult(null);

        using var _ = cancellationToken.Register(() =>
        {
            try
            {
                exitTcs.TrySetCanceled();
                process.Kill();
            }
            catch { }
        });

        await Task.Run(process.Start).ConfigureAwait(false);

        return new ProcessResult(process, exitTcs);
    }

    /// <summary>
    /// 表示进程执行结果。
    /// </summary>
    public class ProcessResult : IDisposable
    {
        private readonly Process _process;
        private readonly TaskCompletionSource<object?> _exitTcs;
        private readonly StringBuilder _standardOutput;

        /// <summary>
        /// 初始化 <see cref="ProcessResult"/> 类的新实例。
        /// </summary>
        /// <param name="process">进程实例。</param>
        /// <param name="exitTcs">退出任务完成源。</param>
        internal ProcessResult(Process process, TaskCompletionSource<object?> exitTcs)
        {
            _process = process;
            _exitTcs = exitTcs;
            _standardOutput = new StringBuilder();
            _ = Task.Run(ReadStandardErrorAsync);
        }

        private async Task ReadStandardErrorAsync() =>
            StandardError = await _process.StandardError.ReadToEndAsync().ConfigureAwait(false);

        /// <summary>
        /// 等待进程退出。
        /// </summary>
        public Task WaitForExitAsync() => _exitTcs.Task;

        /// <summary>
        /// 异步获取标准输出的行。
        /// </summary>
        /// <returns>输出行的异步枚举。</returns>
        public async IAsyncEnumerable<string> GetStandardOutputLinesAsync()
        {
            var output = _process.StandardOutput;
            while (true)
            {
                var line = await output.ReadLineAsync().ConfigureAwait(false);
                if (line == null)
                {
                    await _exitTcs.Task.ConfigureAwait(false);
                    yield break;
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    _standardOutput.AppendLine(line);
                    yield return line;
                }
            }
        }

        /// <summary>
        /// 获取进程退出代码。
        /// </summary>
        public int ExitCode => _process.ExitCode;

        /// <summary>
        /// 获取标准输出内容。
        /// </summary>
        public string StandardOutput => _standardOutput.ToString();

        /// <summary>
        /// 获取标准错误内容。
        /// </summary>
        public string? StandardError { get; private set; }

        /// <summary>
        /// 释放进程资源。
        /// </summary>
        public void Dispose() => _process.Dispose();
    }
}

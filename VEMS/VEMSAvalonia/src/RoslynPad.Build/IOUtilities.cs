#nullable disable

using System.Security;

namespace RoslynPad.Build;

/// <summary>
/// 提供安全的 I/O 操作工具方法，自动处理常见的 I/O 异常。
/// </summary>
internal static class IOUtilities
{
    /// <summary>
    /// 执行 I/O 操作，忽略常见的 I/O 异常。
    /// </summary>
    /// <param name="action">要执行的操作。</param>
    public static void PerformIO(Action action)
    {
        PerformIO<object>(() =>
        {
            action();
            return null;
        });
    }

    /// <summary>
    /// 执行 I/O 操作并返回结果，忽略常见的 I/O 异常。
    /// </summary>
    /// <typeparam name="T">返回类型。</typeparam>
    /// <param name="function">要执行的函数。</param>
    /// <param name="defaultValue">发生异常时返回的默认值。</param>
    /// <returns>操作结果或默认值。</returns>
    public static T PerformIO<T>(Func<T> function, T defaultValue = default)
    {
        try
        {
            return function();
        }
        catch (Exception e) when (IsNormalIOException(e))
        {
        }

        return defaultValue;
    }

    /// <summary>
    /// 异步执行 I/O 操作并返回结果，忽略常见的 I/O 异常。
    /// </summary>
    /// <typeparam name="T">返回类型。</typeparam>
    /// <param name="function">要执行的异步函数。</param>
    /// <param name="defaultValue">发生异常时返回的默认值。</param>
    /// <returns>操作结果或默认值。</returns>
    public static async Task<T> PerformIOAsync<T>(Func<Task<T>> function, T defaultValue = default)
    {
        try
        {
            return await function().ConfigureAwait(false);
        }
        catch (Exception e) when (IsNormalIOException(e))
        {
        }

        return defaultValue;
    }

    /// <summary>
    /// 获取当前目录，如果失败则返回 "."。
    /// </summary>
    public static string CurrentDirectory => PerformIO(Directory.GetCurrentDirectory, ".");

    /// <summary>
    /// 规范化文件路径的大小写。
    /// </summary>
    /// <param name="filename">文件路径。</param>
    /// <returns>规范化后的路径。</returns>
    public static string NormalizeFilePath(string filename)
    {
        var fileInfo = new FileInfo(filename);
        var directoryInfo = fileInfo.Directory;
        return directoryInfo == null
            ? throw new ArgumentException("Invalid path", nameof(filename))
            : Path.Combine(NormalizeDirectory(directoryInfo),
            directoryInfo.GetFiles(fileInfo.Name)[0].Name);
    }

    private static string NormalizeDirectory(DirectoryInfo dirInfo)
    {
        var parentDirInfo = dirInfo.Parent;
        if (parentDirInfo == null)
        {
            return dirInfo.Name;
        }

        return Path.Combine(NormalizeDirectory(parentDirInfo),
            parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
    }

    /// <summary>
    /// 递归枚举目录下的所有文件。
    /// </summary>
    /// <param name="path">目录路径。</param>
    /// <param name="searchPattern">搜索模式。</param>
    /// <returns>文件路径枚举。</returns>
    public static IEnumerable<string> EnumerateFilesRecursive(string path, string searchPattern = "*")
    {
        return EnumerateDirectories(path).Aggregate(
            EnumerateFiles(path, searchPattern),
            (current, directory) => current.Concat(EnumerateFiles(directory, searchPattern)));
    }

    /// <summary>
    /// 安全地读取文件的所有行。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <returns>行枚举。</returns>
    public static IEnumerable<string> ReadLines(string path)
    {
        var lines = PerformIO(() => File.ReadLines(path), []);
        using var enumerator = lines.GetEnumerator();
        while (PerformIO(enumerator.MoveNext))
        {
            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// 异步读取文件的所有文本。
    /// </summary>
    /// <param name="path">文件路径。</param>
    /// <returns>文件内容。</returns>
    public static Task<string> ReadAllTextAsync(string path) =>
        PerformIOAsync(() => ReadAllTextInternalAsync(path), string.Empty);

    private static async Task<string> ReadAllTextInternalAsync(string path)
    {
        using var reader = File.OpenText(path);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 安全地枚举目录中的文件。
    /// </summary>
    /// <param name="path">目录路径。</param>
    /// <param name="searchPattern">搜索模式。</param>
    /// <returns>文件路径枚举。</returns>
    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*")
    {
        var files = PerformIO(() => Directory.EnumerateFiles(path, searchPattern),
            []);

        using var enumerator = files.GetEnumerator();
        while (PerformIO(enumerator.MoveNext))
        {
            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// 安全地枚举子目录。
    /// </summary>
    /// <param name="path">目录路径。</param>
    /// <param name="searchPattern">搜索模式。</param>
    /// <returns>目录路径枚举。</returns>
    public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*")
    {
        var directories = PerformIO(() => Directory.EnumerateDirectories(path, searchPattern), []);

        using var enumerator = directories.GetEnumerator();
        while (PerformIO(enumerator.MoveNext))
        {
            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// 复制目录内容。
    /// </summary>
    /// <param name="source">源目录。</param>
    /// <param name="destination">目标目录。</param>
    /// <param name="overwrite">是否覆盖现有文件。</param>
    /// <param name="recursive">是否递归复制子目录。</param>
    public static void DirectoryCopy(string source, string destination, bool overwrite, bool recursive = true)
    {
        foreach (var file in EnumerateFiles(source))
        {
            var destinationFile = Path.Combine(destination, Path.GetFileName(file));
            FileCopy(file, destinationFile, overwrite);
        }

        if (!recursive)
        {
            return;
        }

        foreach (var directory in EnumerateDirectories(source))
        {
            var destinationDirectory = Path.Combine(destination, Path.GetFileName(directory));
            Directory.CreateDirectory(destinationDirectory);
            DirectoryCopy(directory, destinationDirectory, overwrite);
        }
    }

    /// <summary>
    /// 复制文件，处理加密失败的情况。
    /// </summary>
    /// <param name="source">源文件。</param>
    /// <param name="destination">目标文件。</param>
    /// <param name="overwrite">是否覆盖。</param>
    public static void FileCopy(string source, string destination, bool overwrite)
    {
        const int ERROR_ENCRYPTION_FAILED = unchecked((int)0x80071770);

        try
        {
            File.Copy(source, destination, overwrite);
        }
        catch (IOException ex) when (ex.HResult == ERROR_ENCRYPTION_FAILED)
        {
            using var read = File.OpenRead(source);
            using var write = new FileStream(destination, overwrite ? FileMode.Create : FileMode.CreateNew);
            read.CopyTo(write);
        }
    }

    /// <summary>
    /// 判断是否为常见的 I/O 异常。
    /// </summary>
    /// <param name="e">异常。</param>
    /// <returns>是否为常见 I/O 异常。</returns>
    public static bool IsNormalIOException(Exception e)
    {
        return e is IOException ||
               e is SecurityException ||
               e is ArgumentException ||
               e is UnauthorizedAccessException ||
               e is NotSupportedException ||
               e is InvalidOperationException;
    }
}

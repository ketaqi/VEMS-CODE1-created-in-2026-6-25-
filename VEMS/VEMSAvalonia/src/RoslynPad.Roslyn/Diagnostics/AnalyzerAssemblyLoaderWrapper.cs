using System.Composition;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn.Diagnostics;

/// <summary>
/// 分析器程序集加载器的包装类，实现<see cref="IAnalyzerAssemblyLoader"/>接口并支持资源释放
/// </summary>
[Export(typeof(IAnalyzerAssemblyLoader)), Shared]
internal class AnalyzerAssemblyLoaderWrapper : IAnalyzerAssemblyLoader, IDisposable
{
    /// <summary>
    /// 内部封装的分析器程序集加载器实例
    /// </summary>
    private readonly AnalyzerAssemblyLoader _inner = new();

    /// <summary>
    /// 释放当前实例占用的所有资源
    /// </summary>
    public void Dispose() => _inner.Dispose();

    /// <summary>
    /// 添加依赖项程序集的文件路径
    /// </summary>
    /// <param name="fullPath">依赖项程序集的完整文件路径</param>
    public void AddDependencyLocation(string fullPath) => _inner.AddDependencyLocation(fullPath);

    /// <summary>
    /// 从指定路径加载程序集
    /// </summary>
    /// <param name="fullPath">程序集的完整文件路径</param>
    /// <returns>加载完成的<see cref="Assembly"/>实例</returns>
    public Assembly LoadFromPath(string fullPath) => _inner.LoadFromPath(fullPath);
}

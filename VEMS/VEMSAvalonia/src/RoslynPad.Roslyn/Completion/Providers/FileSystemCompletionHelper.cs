// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace RoslynPad.Roslyn;

/// <summary>
/// 文件系统补全辅助类，用于生成文件/目录的补全项
/// </summary>
/// <param name="folderGlyph">文件夹图标</param>
/// <param name="fileGlyph">文件图标</param>
/// <param name="searchPaths">搜索路径列表（绝对路径）</param>
/// <param name="baseDirectoryOpt">基础目录（可选）</param>
/// <param name="allowableExtensions">允许的文件扩展名列表</param>
/// <param name="itemRules">补全项规则</param>
internal class FileSystemCompletionHelper(
    Glyph folderGlyph,
    Glyph fileGlyph,
    ImmutableArray<string> searchPaths,
    string baseDirectoryOpt,
    ImmutableArray<string> allowableExtensions,
    CompletionItemRules itemRules)
{
    /// <summary>
    /// Windows目录分隔符数组
    /// </summary>
    private static readonly char[] s_windowsDirectorySeparator = ['\\'];

    /// <summary>
    /// 文件夹图标
    /// </summary>
    private readonly Glyph _folderGlyph = folderGlyph;

    /// <summary>
    /// 文件图标
    /// </summary>
    private readonly Glyph _fileGlyph = fileGlyph;

    /// <summary>
    /// 搜索路径列表（绝对路径）
    /// </summary>
    private readonly ImmutableArray<string> _searchPaths = searchPaths;

    /// <summary>
    /// 基础目录（可选）
    /// </summary>
    private readonly string _baseDirectoryOpt = baseDirectoryOpt!;

    /// <summary>
    /// 允许的文件扩展名列表
    /// </summary>
    private readonly ImmutableArray<string> _allowableExtensions = allowableExtensions;

    /// <summary>
    /// 补全项规则
    /// </summary>
    private readonly CompletionItemRules _itemRules = itemRules;

    /// <summary>
    /// 获取当前系统的逻辑驱动器列表
    /// </summary>
    /// <returns>逻辑驱动器路径数组</returns>
    private string[] GetLogicalDrives()
        => IOUtilities.PerformIO(Directory.GetLogicalDrives, []);

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="fullPath">目录完整路径</param>
    /// <returns>目录是否存在</returns>
    private bool DirectoryExists(string fullPath) =>
        Directory.Exists(fullPath);

    /// <summary>
    /// 枚举指定目录下的子目录
    /// </summary>
    /// <param name="fullDirectoryPath">目录完整路径</param>
    /// <returns>子目录路径枚举</returns>
    private IEnumerable<string> EnumerateDirectories(string fullDirectoryPath) =>
        IOUtilities.PerformIO(() => Directory.EnumerateDirectories(fullDirectoryPath), []);

    /// <summary>
    /// 枚举指定目录下的文件
    /// </summary>
    /// <param name="fullDirectoryPath">目录完整路径</param>
    /// <returns>文件路径枚举</returns>
    private IEnumerable<string> EnumerateFiles(string fullDirectoryPath) =>
        IOUtilities.PerformIO(() => Directory.EnumerateFiles(fullDirectoryPath), []);

    /// <summary>
    /// 检查文件系统项是否可见（非隐藏/系统文件）
    /// </summary>
    /// <param name="fullPath">文件/目录完整路径</param>
    /// <returns>是否可见</returns>
    private bool IsVisibleFileSystemEntry(string fullPath) =>
        IOUtilities.PerformIO(() => (File.GetAttributes(fullPath) & (FileAttributes.Hidden | FileAttributes.System)) == 0, false);

    /// <summary>
    /// 创建网络根目录补全项（\\）
    /// </summary>
    /// <returns>网络根补全项</returns>
    private CompletionItem CreateNetworkRoot()
        => CommonCompletionItem.Create(
            "\\\\",
            "",
            glyph: null,
            description: "\\\\".ToSymbolDisplayParts(),
            rules: _itemRules);

    /// <summary>
    /// 创建Unix根目录补全项（/）
    /// </summary>
    /// <returns>Unix根补全项</returns>
    private CompletionItem CreateUnixRoot()
        => CommonCompletionItem.Create(
            "/",
            "",
            glyph: _folderGlyph,
            description: "/".ToSymbolDisplayParts(),
            rules: _itemRules);

    /// <summary>
    /// 创建文件系统项（文件/目录）的补全项
    /// </summary>
    /// <param name="fullPath">文件/目录完整路径</param>
    /// <param name="isDirectory">是否为目录</param>
    /// <returns>文件系统项补全项</returns>
    private CompletionItem CreateFileSystemEntryItem(string fullPath, bool isDirectory)
        => CommonCompletionItem.Create(
            PathUtilities.GetFileName(fullPath),
            "",
            glyph: isDirectory ? _folderGlyph : _fileGlyph,
            description: fullPath.ToSymbolDisplayParts(),
            rules: _itemRules);

    /// <summary>
    /// 创建逻辑驱动器补全项
    /// </summary>
    /// <param name="drive">驱动器路径（如C:\）</param>
    /// <returns>驱动器补全项</returns>
    private CompletionItem CreateLogicalDriveItem(string drive)
        => CommonCompletionItem.Create(
            drive,
            "",
            glyph: _folderGlyph,
            description: drive.ToSymbolDisplayParts(),
            rules: _itemRules);

    /// <summary>
    /// 异步获取指定目录路径的补全项
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>补全项数组</returns>
    public Task<ImmutableArray<CompletionItem>> GetItemsAsync(string directoryPath, CancellationToken cancellationToken)
    {
        return Task.Run(() => GetItems(directoryPath, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// 获取指定目录路径的补全项
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>补全项数组</returns>
    private ImmutableArray<CompletionItem> GetItems(string directoryPath, CancellationToken cancellationToken)
    {
        // 处理Windows仅输入"\"的场景，返回网络根补全项
        if (!PathUtilities.IsUnixLikePlatform && directoryPath.Length == 1 && directoryPath[0] == '\\')
        {
            return [CreateNetworkRoot()];
        }

        var result = ArrayBuilder<CompletionItem>.GetInstance();

        // 根据路径类型处理不同的补全逻辑
        var pathKind = PathUtilities.GetPathKind(directoryPath);
        switch (pathKind)
        {
            case PathKind.Empty:
                // 基础目录补全项
                if (_baseDirectoryOpt != null)
                {
                    result.AddRange(GetItemsInDirectory(_baseDirectoryOpt, cancellationToken));
                }

                // 根目录补全项（Unix/Windows区分）
                if (PathUtilities.IsUnixLikePlatform)
                {
                    result.AddRange(CreateUnixRoot());
                }
                else
                {
                    foreach (var drive in GetLogicalDrives())
                    {
                        result.Add(CreateLogicalDriveItem(drive.TrimEnd(s_windowsDirectorySeparator)));
                    }

                    result.Add(CreateNetworkRoot());
                }

                // 搜索路径下的补全项
                foreach (var searchPath in _searchPaths)
                {
                    result.AddRange(GetItemsInDirectory(searchPath, cancellationToken));
                }

                break;

            case PathKind.Absolute:
            case PathKind.RelativeToCurrentDirectory:
            case PathKind.RelativeToCurrentParent:
            case PathKind.RelativeToCurrentRoot:
                // 解析完整目录路径并获取补全项
                var fullDirectoryPath = FileUtilities.ResolveRelativePath(directoryPath, basePath: null, baseDirectory: _baseDirectoryOpt);
                if (fullDirectoryPath != null)
                {
                    result.AddRange(GetItemsInDirectory(fullDirectoryPath, cancellationToken));
                }
                else
                {
                    // 无效路径清空结果
                    result.Clear();
                }

                break;

            case PathKind.Relative:
                // 基础目录下的相对路径补全项
                if (_baseDirectoryOpt != null)
                {
                    result.AddRange(GetItemsInDirectory(PathUtilities.CombineAbsoluteAndRelativePaths(_baseDirectoryOpt, directoryPath)!, cancellationToken));
                }

                // 搜索路径下的相对路径补全项
                foreach (var searchPath in _searchPaths)
                {
                    result.AddRange(GetItemsInDirectory(PathUtilities.CombineAbsoluteAndRelativePaths(searchPath, directoryPath)!, cancellationToken));
                }

                break;

            case PathKind.RelativeToDriveDirectory:
                // 仅处理"X:"格式的驱动器路径
                if (directoryPath.Length == 2)
                {
                    result.Add(CreateLogicalDriveItem(directoryPath));
                }

                break;

            default:
                throw ExceptionUtilities.UnexpectedValue(pathKind);
        }

        return result.ToImmutableAndFree();
    }

    /// <summary>
    /// 获取指定目录下的文件/目录补全项
    /// </summary>
    /// <param name="fullDirectoryPath">目录完整路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>补全项枚举</returns>
    private IEnumerable<CompletionItem> GetItemsInDirectory(string fullDirectoryPath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 目录不存在则返回空
        if (!DirectoryExists(fullDirectoryPath))
        {
            yield break;
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 添加目录补全项（仅可见目录）
        foreach (var directory in EnumerateDirectories(fullDirectoryPath))
        {
            if (IsVisibleFileSystemEntry(directory))
            {
                yield return CreateFileSystemEntryItem(directory, isDirectory: true);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 添加文件补全项（仅允许的扩展名+可见文件）
        foreach (var file in EnumerateFiles(fullDirectoryPath))
        {
            if (_allowableExtensions.Length != 0 &&
                !_allowableExtensions.Contains(
                    PathUtilities.GetExtension(file),
                    PathUtilities.IsUnixLikePlatform ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (IsVisibleFileSystemEntry(file))
            {
                yield return CreateFileSystemEntryItem(file, isDirectory: false);
            }
        }
    }
}

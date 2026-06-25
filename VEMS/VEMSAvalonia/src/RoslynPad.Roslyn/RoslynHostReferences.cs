using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Roslyn.Utilities;

namespace RoslynPad.Roslyn;

#pragma warning disable IL3000 // Assembly.Location is fine here

/// <summary>
/// 封装 Roslyn 宿主所需的程序集引用、命名空间导入等信息
/// </summary>
public class RoslynHostReferences
{
    /// <summary>
    /// 获取空的 RoslynHostReferences 实例
    /// </summary>
    public static RoslynHostReferences Empty { get; } = new(
        [],
        ImmutableDictionary<string, string>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase),
        []);

    /// <summary>
    /// 获取仅包含默认命名空间（无程序集）的 RoslynHostReferences 实例，适配所有框架
    /// </summary>
    public static RoslynHostReferences NamespaceDefault { get; } = Empty.With(imports: [
        "System",
        "System.Threading",
        "System.Threading.Tasks",
        "System.Collections",
        "System.Collections.Generic",
        "System.Text",
        "System.Text.RegularExpressions",
        "System.Linq",
        "System.IO",
        "System.Reflection",
    ]);

    /// <summary>
    /// 创建当前实例的副本，并追加指定的引用、导入等信息
    /// </summary>
    /// <param name="references">要追加的元数据引用集合</param>
    /// <param name="imports">要追加的命名空间导入集合</param>
    /// <param name="assemblyReferences">要追加的程序集引用集合</param>
    /// <param name="assemblyPathReferences">要追加的程序集文件路径引用集合</param>
    /// <param name="typeNamespaceImports">要导入其命名空间的类型集合</param>
    /// <returns>包含追加信息的新 RoslynHostReferences 实例</returns>
    public RoslynHostReferences With(IEnumerable<MetadataReference>? references = null, IEnumerable<string>? imports = null,
        IEnumerable<Assembly>? assemblyReferences = null, IEnumerable<string>? assemblyPathReferences = null, IEnumerable<Type>? typeNamespaceImports = null)
    {
        var referenceLocations = _referenceLocations;
        var importsArray = Imports.AddRange(imports!.WhereNotNull());

        var locations =
            assemblyReferences!.WhereNotNull().Select(c => c.Location).Concat(
            assemblyPathReferences!.WhereNotNull());

        foreach (var location in locations)
        {
            referenceLocations = referenceLocations.SetItem(location, string.Empty);
        }

        foreach (var type in typeNamespaceImports!.WhereNotNull())
        {
            importsArray = importsArray.Add(type!.Namespace!);
            var location = type.Assembly.Location;
            referenceLocations = referenceLocations.SetItem(location, string.Empty);
        }

        return new RoslynHostReferences(
            _references.AddRange(references!.WhereNotNull()),
            referenceLocations,
            importsArray);
    }

    /// <summary>
    /// 构造函数（私有），初始化 RoslynHostReferences 实例
    /// </summary>
    /// <param name="references">元数据引用集合</param>
    /// <param name="referenceLocations">程序集路径与空字符串的映射字典（忽略值，仅记录路径）</param>
    /// <param name="imports">命名空间导入集合</param>
    private RoslynHostReferences(
        ImmutableArray<MetadataReference> references,
        ImmutableDictionary<string, string> referenceLocations,
        ImmutableArray<string> imports)
    {
        _references = references;
        _referenceLocations = referenceLocations;
        Imports = imports;
    }

    /// <summary>
    /// 存储元数据引用集合
    /// </summary>
    private readonly ImmutableArray<MetadataReference> _references;

    /// <summary>
    /// 存储程序集路径映射字典
    /// </summary>
    private readonly ImmutableDictionary<string, string> _referenceLocations;

    /// <summary>
    /// 获取命名空间导入集合
    /// </summary>
    public ImmutableArray<string> Imports { get; }

    /// <summary>
    /// 获取合并后的元数据引用集合
    /// </summary>
    /// <param name="documentationProviderFactory">文档提供程序工厂方法</param>
    /// <returns>包含内置引用和路径引用的元数据引用数组</returns>
    public ImmutableArray<MetadataReference> GetReferences(Func<string, DocumentationProvider>? documentationProviderFactory = null) =>
        Enumerable.Concat(_references, Enumerable.Select(_referenceLocations, c => MetadataReference.CreateFromFile(c.Key, documentation: documentationProviderFactory?.Invoke(c.Key))))
            .ToImmutableArray();
}

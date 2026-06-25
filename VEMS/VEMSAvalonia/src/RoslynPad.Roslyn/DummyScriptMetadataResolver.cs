using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace RoslynPad.Roslyn;

/// <summary>
/// 虚拟的脚本元数据解析器，用于处理脚本元数据引用的解析
/// 所有解析操作均返回空/默认值，仅作为占位实现
/// </summary>
public class DummyScriptMetadataResolver : MetadataReferenceResolver
{
    /// <summary>
    /// 获取 <see cref="DummyScriptMetadataResolver"/> 的单例实例
    /// </summary>
    public static DummyScriptMetadataResolver Instance { get; } = new DummyScriptMetadataResolver();

    /// <summary>
    /// 私有构造函数，确保类的单例特性
    /// </summary>
    private DummyScriptMetadataResolver() { }

    /// <summary>
    /// 确定指定对象是否等于当前实例（仅引用相等）
    /// </summary>
    /// <param name="other">要与当前实例比较的对象</param>
    /// <returns>如果对象引用相同则为 true，否则为 false</returns>
    public override bool Equals(object? other) => ReferenceEquals(this, other);

    /// <summary>
    /// 获取当前实例的哈希码
    /// </summary>
    /// <returns>当前实例的哈希码</returns>
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    /// <summary>
    /// 获取一个值，指示是否应解析缺失的程序集
    /// </summary>
    public override bool ResolveMissingAssemblies => false;

    /// <summary>
    /// 解析缺失的程序集引用（此处返回 null，不执行实际解析）
    /// </summary>
    /// <param name="definition">元数据引用定义</param>
    /// <param name="referenceIdentity">要解析的程序集标识</param>
    /// <returns>始终返回 null，表示不解析缺失的程序集</returns>
    public override PortableExecutableReference? ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity) => null;

    /// <summary>
    /// 解析指定的元数据引用（此处返回空数组，不执行实际解析）
    /// </summary>
    /// <param name="reference">要解析的引用字符串</param>
    /// <param name="baseFilePath">基文件路径（未使用）</param>
    /// <param name="properties">元数据引用属性（未使用）</param>
    /// <returns>空的不可变数组，表示无解析结果</returns>
    public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties) =>
        [];
}

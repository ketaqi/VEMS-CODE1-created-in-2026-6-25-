using System.Collections.Immutable;
using System.Reflection;
using Roslyn.Utilities;
using System.Reflection.Metadata;

namespace RoslynPad.Roslyn;

/// <summary>
/// 元数据操作工具类，提供程序集路径获取、类型加载等元数据相关功能
/// </summary>
internal class MetadataUtil
{
    /// <summary>
    /// 获取指定程序集的文件路径（基于应用程序基目录）
    /// </summary>
    /// <param name="assembly">目标程序集</param>
    /// <returns>程序集对应的DLL文件路径</returns>
    public static string GetAssemblyPath(Assembly assembly) => Path.Combine(AppContext.BaseDirectory, assembly.GetName().Name + ".dll");

    /// <summary>
    /// 根据指定的命名空间集合加载程序集中的类型
    /// </summary>
    /// <param name="assembly">要加载类型的程序集</param>
    /// <param name="namespaces">筛选类型的命名空间集合</param>
    /// <returns>匹配命名空间的类型列表</returns>
    public static IReadOnlyList<Type> LoadTypesByNamespaces(Assembly assembly, params string[] namespaces) =>
        LoadTypesBy(assembly, t => namespaces.Contains(t.Namespace));

    /// <summary>
    /// 根据自定义谓词筛选加载程序集中的类型
    /// </summary>
    /// <param name="assembly">要加载类型的程序集</param>
    /// <param name="predicate">筛选类型的谓词函数</param>
    /// <returns>符合谓词条件的类型列表</returns>
    public static unsafe IReadOnlyList<Type> LoadTypesBy(Assembly assembly, Func<TypeInfo, bool> predicate)
    {
        // 尝试获取程序集的原始元数据，获取失败则返回空列表
        if (!assembly.TryGetRawMetadata(out var metadata, out var length))
        {
            return [];
        }

        var types = new List<Type>();

        // 创建元数据读取器，遍历所有类型定义
        MetadataReader reader = new(metadata, length);
        foreach (var typeDefHandle in reader.TypeDefinitions)
        {
            var typeDef = reader.GetTypeDefinition(typeDefHandle);
            var typeInfo = new TypeInfo(reader.GetString(typeDef.Namespace), reader.GetString(typeDef.Name));

            // 符合筛选条件的类型，通过全名加载并加入结果列表
            if (predicate(typeInfo))
            {
                var type = assembly.GetType(typeInfo.FullName);
                if (type is not null)
                {
                    types.Add(type);
                }
            }
        }

        return types;
    }

    /// <summary>
    /// 类型信息记录，包含命名空间和名称，以及拼接后的完整名称
    /// </summary>
    /// <param name="Namespace">类型所属命名空间</param>
    /// <param name="Name">类型名称</param>
    public record TypeInfo(string Namespace, string Name)
    {
        private string? _fullName;

        /// <summary>
        /// 获取类型的完整名称（命名空间.类型名）
        /// </summary>
        public string FullName => _fullName ??= $"{Namespace}.{Name}";
    }
}

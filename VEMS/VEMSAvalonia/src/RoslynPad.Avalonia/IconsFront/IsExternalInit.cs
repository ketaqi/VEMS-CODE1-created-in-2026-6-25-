using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 保留类型，用于支持 C# 9.0 的 init 访问器功能。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类型在 . NET 5. 0+ 中由运行时提供。
    /// 对于面向较早版本 .NET 的项目，需要手动定义此类型以启用 <c>init</c> 访问器。
    /// </para>
    /// <para>
    /// 此类型仅供编译器使用，不应在用户代码中直接引用。
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}

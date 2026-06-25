using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynPad.Build;

/// <summary>
/// 提供编译所需的代码片段和语法树生成工具。
/// </summary>
internal static class BuildCode
{
    /// <summary>
    /// 脚本模式下的运行时初始化代码。
    /// </summary>
    public const string ScriptInit = "RoslynPad.Runtime.RuntimeInitializer.Initialize();";

    /// <summary>
    /// ModuleInitializer 特性文件名（用于 .NET 5 以下版本）。
    /// </summary>
    public const string ModuleInitAttributeFileName = "ModuleInitializerAttribute.cs";

    /// <summary>
    /// ModuleInitializer 特性定义代码（用于 .NET 5 以下版本）。
    /// </summary>
    public const string ModuleInitAttribute = @"
            using System;

            namespace System.Runtime.CompilerServices
            {
                [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
                public sealed class ModuleInitializerAttribute : Attribute { }
            }
        ";

    /// <summary>
    /// 模块初始化器文件名。
    /// </summary>
    public const string ModuleInitFileName = "ModuleInitializer.cs";

    /// <summary>
    /// 模块初始化器代码，用于在程序集加载时自动初始化运行时。
    /// </summary>
    public const string ModuleInit = @"
            internal static class ModuleInitializer
            {
                [System.Runtime.CompilerServices.ModuleInitializer]
                internal static void Initialize() =>
                    RoslynPad.Runtime.RuntimeInitializer.Initialize();
            }
        ";

    /// <summary>
    /// 为表达式语句生成 .Dump() 调用，用于在脚本末尾自动输出结果。
    /// </summary>
    /// <param name="statement">要包装的表达式语句。</param>
    /// <returns>包含 .Dump() 调用的全局语句语法节点。</returns>
    public static GlobalStatementSyntax GetDumpCall(ExpressionStatementSyntax statement) =>
        GlobalStatement(
            ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    statement.Expression,
                    IdentifierName("Dump")))));
}

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace RoslynPad.Roslyn;

/// <summary>
/// C#代码编译器，负责将语法树编译为程序集
/// </summary>
/// <param name="syntaxTrees">要编译的语法树列表</param>
/// <param name="parseOptions">C#解析选项</param>
/// <param name="outputKind">输出程序集类型（默认：动态链接库）</param>
/// <param name="platform">目标平台（默认：AnyCpu）</param>
/// <param name="references">元数据引用集合（可选）</param>
/// <param name="usings">默认using指令集合（可选）</param>
/// <param name="workingDirectory">工作目录（可选）</param>
/// <param name="sourceResolver">源引用解析器（可选）</param>
/// <param name="optimizationLevel">优化级别（默认：调试）</param>
/// <param name="checkOverflow">是否检查溢出（默认：false）</param>
/// <param name="allowUnsafe">是否允许不安全代码（默认：true）</param>
internal sealed class Compiler(
    ImmutableList<SyntaxTree> syntaxTrees,
    CSharpParseOptions parseOptions,
    OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
    Platform platform = Platform.AnyCpu,
    IEnumerable<MetadataReference>? references = null,
    IEnumerable<string>? usings = null,
    string? workingDirectory = null,
    SourceReferenceResolver? sourceResolver = null,
    OptimizationLevel optimizationLevel = OptimizationLevel.Debug,
    bool checkOverflow = false,
    bool allowUnsafe = true)
{
    /// <summary>
    /// 要编译的语法树列表
    /// </summary>
    public ImmutableList<SyntaxTree> SyntaxTrees { get; } = syntaxTrees;

    /// <summary>
    /// 输出程序集类型
    /// </summary>
    public OutputKind OutputKind { get; } = outputKind;

    /// <summary>
    /// 目标平台
    /// </summary>
    public Platform Platform { get; } = platform;

    /// <summary>
    /// 元数据引用集合
    /// </summary>
    public ImmutableArray<MetadataReference> References { get; } = references?.AsImmutable() ?? [];

    /// <summary>
    /// 源引用解析器
    /// </summary>
    public SourceReferenceResolver SourceResolver { get; } = sourceResolver ??
                         (workingDirectory != null
                             ? new SourceFileResolver([], workingDirectory)
                             : SourceFileResolver.Default);

    /// <summary>
    /// 默认using指令集合
    /// </summary>
    public ImmutableArray<string> Usings { get; } = usings?.AsImmutable() ?? [];

    /// <summary>
    /// C#解析选项
    /// </summary>
    public CSharpParseOptions ParseOptions { get; } = parseOptions;

    /// <summary>
    /// 编译优化级别
    /// </summary>
    public OptimizationLevel OptimizationLevel { get; } = optimizationLevel;

    /// <summary>
    /// 是否检查数值溢出
    /// </summary>
    public bool CheckOverflow { get; } = checkOverflow;

    /// <summary>
    /// 是否允许不安全代码
    /// </summary>
    public bool AllowUnsafe { get; } = allowUnsafe;

    /// <summary>
    /// 编译并保存程序集到指定路径
    /// </summary>
    /// <param name="assemblyPath">程序集输出路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>编译过程中的诊断信息（错误和警告）</returns>
    public ImmutableArray<Diagnostic> CompileAndSaveAssembly(string assemblyPath, CancellationToken cancellationToken = default)
    {
        var compilation = GetCompilationFromCode(Path.GetFileNameWithoutExtension(assemblyPath));

        var diagnostics = compilation.GetParseDiagnostics(cancellationToken);
        if (!diagnostics.IsEmpty)
        {
            return diagnostics;
        }

        var diagnosticsBag = new DiagnosticBag();
        SaveAssembly(assemblyPath, compilation, diagnosticsBag, cancellationToken);
        return GetDiagnostics(diagnosticsBag, includeWarnings: true);
    }

    /// <summary>
    /// 将编译结果保存为程序集文件
    /// </summary>
    /// <param name="assemblyPath">程序集输出路径</param>
    /// <param name="compilation">编译实例</param>
    /// <param name="diagnostics">诊断信息容器</param>
    /// <param name="cancellationToken">取消令牌</param>
    private static void SaveAssembly(
        string assemblyPath,
        Compilation compilation,
        DiagnosticBag diagnostics,
        CancellationToken cancellationToken)
    {
        using var peStream = File.OpenWrite(assemblyPath);
        using var pdbStream = File.OpenWrite(Path.ChangeExtension(assemblyPath, "pdb"));
        var emitResult = compilation.Emit(
            peStream: peStream,
            pdbStream: pdbStream,
            options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb),
            cancellationToken: cancellationToken);

        diagnostics.AddRange(emitResult.Diagnostics);
    }

    /// <summary>
    /// 根据语法树创建C#编译实例
    /// </summary>
    /// <param name="assemblyName">程序集名称</param>
    /// <returns>配置好的CSharpCompilation实例</returns>
    private CSharpCompilation GetCompilationFromCode(string assemblyName)
    {
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind,
            mainTypeName: null,
            scriptClassName: ParseOptions.Kind == SourceCodeKind.Script ? "Program" : null,
            usings: Usings,
            optimizationLevel: OptimizationLevel,
            checkOverflow: CheckOverflow,
            allowUnsafe: AllowUnsafe,
            platform: Platform,
            warningLevel: 4,
            deterministic: true,
            xmlReferenceResolver: null,
            sourceReferenceResolver: SourceResolver,
            assemblyIdentityComparer: AssemblyIdentityComparer.Default,
            nullableContextOptions: NullableContextOptions.Enable
        );

        return CSharpCompilation.Create(
             assemblyName,
             SyntaxTrees,
             References,
             compilationOptions);
    }

    /// <summary>
    /// 从诊断容器中获取诊断信息（错误和可选的警告）
    /// </summary>
    /// <param name="diagnostics">诊断信息容器</param>
    /// <param name="includeWarnings">是否包含警告信息</param>
    /// <returns>筛选后的诊断信息不可变数组</returns>
    private static ImmutableArray<Diagnostic> GetDiagnostics(DiagnosticBag diagnostics, bool includeWarnings)
    {
        if (diagnostics.IsEmptyWithoutResolution)
        {
            return [];
        }

        return diagnostics.AsEnumerable().Where(d =>
            d.Severity == DiagnosticSeverity.Error || (includeWarnings && d.Severity == DiagnosticSeverity.Warning)).AsImmutable();
    }

    /// <summary>
    /// 诊断信息容器，用于线程安全地存储编译过程中的诊断信息
    /// </summary>
    private class DiagnosticBag
    {
        private ConcurrentQueue<Diagnostic>? _lazyBag;

        /// <summary>
        /// 获取一个值，指示容器是否为空（未解析状态）
        /// </summary>
        public bool IsEmptyWithoutResolution => _lazyBag?.IsEmpty != false;

        /// <summary>
        /// 获取线程安全的诊断信息队列（延迟初始化）
        /// </summary>
        private ConcurrentQueue<Diagnostic> Bag
        {
            get
            {
                var bag = _lazyBag;
                if (bag != null)
                {
                    return bag;
                }

                var newBag = new ConcurrentQueue<Diagnostic>();
                return Interlocked.CompareExchange(ref _lazyBag, newBag, null) ?? newBag;
            }
        }

        /// <summary>
        /// 添加一组诊断信息到容器中
        /// </summary>
        /// <typeparam name="T">诊断信息类型，必须继承自Diagnostic</typeparam>
        /// <param name="diagnostics">要添加的诊断信息数组</param>
        public void AddRange<T>(ImmutableArray<T> diagnostics) where T : Diagnostic
        {
            if (!diagnostics.IsDefaultOrEmpty)
            {
                var bag = Bag;
                foreach (var t in diagnostics)
                {
                    bag.Enqueue(t);
                }
            }
        }

        /// <summary>
        /// 获取容器中的所有诊断信息（过滤掉Void类型的诊断）
        /// </summary>
        /// <returns>诊断信息枚举器</returns>
        public IEnumerable<Diagnostic> AsEnumerable()
        {
            var bag = Bag;

            var foundVoid = bag.Any(diagnostic => diagnostic.Severity == DiagnosticSeverityVoid);

            return foundVoid
                ? AsEnumerableFiltered()
                : bag;
        }

        /// <summary>
        /// 清空容器中的诊断信息
        /// </summary>
        internal void Clear()
        {
            var bag = _lazyBag;
            if (bag != null)
            {
                _lazyBag = null;
            }
        }

        /// <summary>
        /// 表示无效的诊断级别（用于过滤）
        /// </summary>
        private static DiagnosticSeverity DiagnosticSeverityVoid => ~DiagnosticSeverity.Info;

        /// <summary>
        /// 获取过滤掉Void类型诊断后的枚举器
        /// </summary>
        /// <returns>过滤后的诊断信息枚举器</returns>
        private IEnumerable<Diagnostic> AsEnumerableFiltered() =>
            Bag.Where(diagnostic => diagnostic.Severity != DiagnosticSeverityVoid);
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using RoslynPad.Roslyn.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Reflection;
using AnalyzerReference = Microsoft.CodeAnalysis.Diagnostics.AnalyzerReference;
using AnalyzerFileReference = Microsoft.CodeAnalysis.Diagnostics.AnalyzerFileReference;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn;

/// <summary>
/// Roslyn 宿主核心类，管理工作区、编译选项、程序集引用、分析器等核心能力
/// </summary>
public class RoslynHost : IRoslynHost
{
    /// <summary>
    /// 默认预处理器符号集合
    /// </summary>
    internal static readonly ImmutableArray<string> PreprocessorSymbols =
        ["TRACE", "DEBUG"];

    /// <summary>
    /// 默认组合程序集集合
    /// </summary>
    internal static readonly ImmutableArray<Assembly> DefaultCompositionAssemblies =
        [
            typeof(WorkspacesResources).Assembly,
            typeof(CSharpWorkspaceResources).Assembly,
            typeof(FeaturesResources).Assembly,
            typeof(CSharpFeaturesResources).Assembly,
            typeof(RoslynHost).Assembly,
        ];

    /// <summary>
    /// 默认组合类型集合
    /// </summary>
    internal static readonly ImmutableArray<Type> DefaultCompositionTypes =
        DefaultCompositionAssemblies.SelectMany(t => t.DefinedTypes).Select(t => t.AsType())
        .Concat(GetDiagnosticCompositionTypes())
        .ToImmutableArray();

    /// <summary>
    /// 获取诊断相关的组合类型集合
    /// </summary>
    /// <returns>诊断组件类型集合</returns>
    private static IEnumerable<Type> GetDiagnosticCompositionTypes() => MetadataUtil.LoadTypesByNamespaces(
        typeof(Microsoft.CodeAnalysis.CodeFixes.ICodeFixService).Assembly,
        "Microsoft.CodeAnalysis.Diagnostics",
        "Microsoft.CodeAnalysis.CodeFixes");

    /// <summary>
    /// 文档ID与工作区的映射字典
    /// </summary>
    private readonly ConcurrentDictionary<DocumentId, RoslynWorkspace> _workspaces;

    /// <summary>
    /// 文档提供程序服务实例
    /// </summary>
    private readonly IDocumentationProviderService _documentationProviderService;

    /// <summary>
    /// MEF 组合上下文
    /// </summary>
    private readonly CompositionHost _compositionContext;

    /// <summary>
    /// 获取宿主服务实例
    /// </summary>
    public HostServices HostServices { get; }

    /// <summary>
    /// 获取解析选项实例
    /// </summary>
    public ParseOptions ParseOptions { get; }

    /// <summary>
    /// 获取默认元数据引用集合
    /// </summary>
    public ImmutableArray<MetadataReference> DefaultReferences { get; }

    /// <summary>
    /// 获取默认命名空间导入集合
    /// </summary>
    public ImmutableArray<string> DefaultImports { get; }

    /// <summary>
    /// 获取禁用的诊断规则ID集合
    /// </summary>
    public ImmutableHashSet<string> DisabledDiagnostics { get; }

    /// <summary>
    /// 获取分析器配置文件路径集合
    /// </summary>
    public ImmutableArray<string> AnalyzerConfigFiles { get; }

    /// <summary>
    /// 初始化 RoslynHost 实例
    /// </summary>
    /// <param name="additionalAssemblies">额外的组合程序集</param>
    /// <param name="references">宿主引用配置</param>
    /// <param name="disabledDiagnostics">禁用的诊断规则ID集合</param>
    /// <param name="analyzerConfigFiles">分析器配置文件路径集合</param>
    public RoslynHost(IEnumerable<Assembly>? additionalAssemblies = null,
        RoslynHostReferences? references = null,
        ImmutableHashSet<string>? disabledDiagnostics = null,
        ImmutableArray<string>? analyzerConfigFiles = null)
    {
        references ??= RoslynHostReferences.Empty;

        _workspaces = [];

        var partTypes = GetDefaultCompositionTypes();

        if (additionalAssemblies != null)
        {
            partTypes = partTypes.Concat(additionalAssemblies.SelectMany(a => a.DefinedTypes).Select(t => t.AsType()));
        }

        _compositionContext = new ContainerConfiguration()
            .WithParts(partTypes)
            .CreateContainer();

        HostServices = MefHostServices.Create(_compositionContext);

        ParseOptions = CreateDefaultParseOptions();

        _documentationProviderService = GetService<IDocumentationProviderService>();

        DefaultReferences = references.GetReferences(DocumentationProviderFactory);
        DefaultImports = references.Imports;

        DisabledDiagnostics = disabledDiagnostics ?? [];
        AnalyzerConfigFiles = analyzerConfigFiles ?? [];
    }

    /// <summary>
    /// 获取文档提供程序工厂方法
    /// </summary>
    public Func<string, DocumentationProvider> DocumentationProviderFactory => _documentationProviderService.GetDocumentationProvider;

    /// <summary>
    /// 获取默认组合类型集合（可重写）
    /// </summary>
    /// <returns>组合类型集合</returns>
    protected virtual IEnumerable<Type> GetDefaultCompositionTypes() => DefaultCompositionTypes;

    /// <summary>
    /// 创建默认解析选项（可重写）
    /// </summary>
    /// <returns>C# 解析选项实例</returns>
    protected virtual ParseOptions CreateDefaultParseOptions() => new CSharpParseOptions(
        preprocessorSymbols: PreprocessorSymbols,
        languageVersion: LanguageVersion.Preview);

    /// <summary>
    /// 创建元数据引用
    /// </summary>
    /// <param name="location">程序集文件路径</param>
    /// <returns>元数据引用实例</returns>
    public MetadataReference CreateMetadataReference(string location) => MetadataReference.CreateFromFile(location,
        documentation: _documentationProviderService.GetDocumentationProvider(location));

    /// <summary>
    /// 获取指定类型的 MEF 导出服务
    /// </summary>
    /// <typeparam name="TService">服务类型</typeparam>
    /// <returns>服务实例</returns>
    public TService GetService<TService>() => _compositionContext.GetExport<TService>();

    /// <summary>
    /// 获取指定文档所属工作区的服务实例
    /// </summary>
    /// <typeparam name="TService">工作区服务类型</typeparam>
    /// <param name="documentId">文档 ID</param>
    /// <returns>工作区服务实例</returns>
    public TService GetWorkspaceService<TService>(DocumentId documentId) where TService : IWorkspaceService =>
        _workspaces[documentId].Services.GetRequiredService<TService>();

    /// <summary>
    /// 为指定项目添加元数据引用（可重写）
    /// </summary>
    /// <param name="projectId">项目 ID</param>
    /// <param name="assemblyIdentity">程序集标识</param>
    protected internal virtual void AddMetadataReference(ProjectId projectId, AssemblyIdentity assemblyIdentity)
    {
    }

    /// <summary>
    /// 关闭指定工作区并释放资源
    /// </summary>
    /// <param name="workspace">要关闭的工作区</param>
    public void CloseWorkspace(RoslynWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        foreach (var documentId in workspace.CurrentSolution.Projects.SelectMany(p => p.DocumentIds))
        {
            _workspaces.TryRemove(documentId, out _);
        }

        workspace.Dispose();
    }

    /// <summary>
    /// 创建新的 Roslyn 工作区（可重写）
    /// </summary>
    /// <returns>新的工作区实例</returns>
    public virtual RoslynWorkspace CreateWorkspace()
    {
        var workspace = new RoslynWorkspace(HostServices, roslynHost: this);
        // 创建诊断更新器（在打开任何文档前）
        var diagnosticsUpdater = workspace.Services.GetRequiredService<IDiagnosticsUpdater>();
        diagnosticsUpdater.DisabledDiagnostics = DisabledDiagnostics;
        return workspace;
    }

    /// <summary>
    /// 关闭指定文档并清理相关资源
    /// </summary>
    /// <param name="documentId">要关闭的文档 ID</param>
    public void CloseDocument(DocumentId documentId)
    {
        ArgumentNullException.ThrowIfNull(documentId);

        if (_workspaces.TryGetValue(documentId, out var workspace))
        {
            workspace.CloseDocument(documentId);

            var document = workspace.CurrentSolution.GetDocument(documentId);

            if (document != null)
            {
                var solution = document.Project.RemoveDocument(documentId).Solution;

                if (!solution.Projects.SelectMany(d => d.DocumentIds).Any())
                {
                    if (_workspaces.TryRemove(documentId, out workspace))
                    {
                        workspace.Dispose();
                    }
                }
                else
                {
                    workspace.SetCurrentSolution(solution);
                }
            }
        }
    }

    /// <summary>
    /// 获取指定 ID 的文档实例
    /// </summary>
    /// <param name="documentId">文档 ID</param>
    /// <returns>文档实例（不存在则返回 null）</returns>
    public Document? GetDocument(DocumentId documentId)
    {
        ArgumentNullException.ThrowIfNull(documentId);

        return _workspaces.TryGetValue(documentId, out var workspace)
            ? workspace.CurrentSolution.GetDocument(documentId)
            : null;
    }

    /// <summary>
    /// 创建新工作区并添加文档
    /// </summary>
    /// <param name="args">文档创建参数</param>
    /// <returns>新文档的 ID</returns>
    public DocumentId AddDocument(DocumentCreationArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        return AddDocument(CreateWorkspace(), args);
    }

    /// <summary>
    /// 为关联文档添加新的相关文档
    /// </summary>
    /// <param name="relatedDocumentId">关联文档的 ID</param>
    /// <param name="args">文档创建参数</param>
    /// <param name="addProjectReference">是否添加项目引用</param>
    /// <returns>新文档的 ID</returns>
    /// <exception cref="ArgumentException">关联文档的工作区不存在时抛出</exception>
    public DocumentId AddRelatedDocument(DocumentId relatedDocumentId, DocumentCreationArgs args, bool addProjectReference = true)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!_workspaces.TryGetValue(relatedDocumentId, out var workspace))
        {
            throw new ArgumentException("Unable to locate the document's workspace", nameof(relatedDocumentId));
        }

        var documentId = AddDocument(workspace, args,
            addProjectReference ? workspace.CurrentSolution.GetDocument(relatedDocumentId) : null);

        return documentId;
    }

    /// <summary>
    /// 向指定工作区添加文档
    /// </summary>
    /// <param name="workspace">目标工作区</param>
    /// <param name="args">文档创建参数</param>
    /// <param name="previousDocument">关联的前置文档（可选）</param>
    /// <returns>新文档的 ID</returns>
    private DocumentId AddDocument(RoslynWorkspace workspace, DocumentCreationArgs args, Document? previousDocument = null)
    {
        var solution = workspace.CurrentSolution;

        if (previousDocument == null)
        {
            solution = solution.AddAnalyzerReferences(GetSolutionAnalyzerReferences());
        }

        var project = CreateProject(solution, args,
            CreateCompilationOptions(args, previousDocument == null), previousDocument?.Project);
        var document = CreateDocument(project, args);
        var documentId = document.Id;

        workspace.SetCurrentSolution(document.Project.Solution);
        workspace.OpenDocument(documentId, args.SourceTextContainer);

        _workspaces.TryAdd(documentId, workspace);

        var onTextUpdated = args.OnTextUpdated;
        if (onTextUpdated != null)
        {
            workspace.ApplyingTextChange += OnTextUpdated;
        }

        return documentId;

        void OnTextUpdated(DocumentId id, SourceText sourceText)
        {
            if (documentId == id)
            {
                onTextUpdated?.Invoke(sourceText);
            }
        }
    }

    /// <summary>
    /// 获取解决方案级别的分析器引用集合（可重写）
    /// </summary>
    /// <returns>分析器引用集合</returns>
    protected virtual IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences()
    {
        var loader = GetService<IAnalyzerAssemblyLoader>();
        yield return new AnalyzerFileReference(MetadataUtil.GetAssemblyPath(typeof(Compilation).Assembly), loader);
        yield return new AnalyzerFileReference(MetadataUtil.GetAssemblyPath(typeof(CSharpResources).Assembly), loader);
        yield return new AnalyzerFileReference(MetadataUtil.GetAssemblyPath(typeof(FeaturesResources).Assembly), loader);
        yield return new AnalyzerFileReference(MetadataUtil.GetAssemblyPath(typeof(CSharpFeaturesResources).Assembly), loader);
    }

    /// <summary>
    /// 更新文档实例
    /// </summary>
    /// <param name="document">更新后的文档实例</param>
    public void UpdateDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (!_workspaces.TryGetValue(document.Id, out var workspace))
        {
            return;
        }

        workspace.TryApplyChanges(document.Project.Solution);
    }

    /// <summary>
    /// 创建编译选项（可重写）
    /// </summary>
    /// <param name="args">文档创建参数</param>
    /// <param name="addDefaultImports">是否添加默认命名空间导入</param>
    /// <returns>C# 编译选项实例</returns>
    protected virtual CompilationOptions CreateCompilationOptions(DocumentCreationArgs args, bool addDefaultImports)
    {
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
            usings: addDefaultImports ? DefaultImports : [],
            allowUnsafe: true,
            sourceReferenceResolver: new SourceFileResolver([], args.WorkingDirectory),
            // 所有 #r 引用由编辑器/msbuild 解析
            metadataReferenceResolver: DummyScriptMetadataResolver.Instance,
            nullableContextOptions: NullableContextOptions.Enable);
        return compilationOptions;
    }

    /// <summary>
    /// 创建文档实例（可重写）
    /// </summary>
    /// <param name="project">所属项目</param>
    /// <param name="args">文档创建参数</param>
    /// <returns>文档实例</returns>
    protected virtual Document CreateDocument(Project project, DocumentCreationArgs args)
    {
        var id = DocumentId.CreateNewId(project.Id);
        var solution = project.Solution.AddDocument(id, args.Name ?? project.Name, args.SourceTextContainer.CurrentText);
        return solution.GetDocument(id)!;
    }

    /// <summary>
    /// 创建项目实例（可重写）
    /// </summary>
    /// <param name="solution">所属解决方案</param>
    /// <param name="args">文档创建参数（用于派生项目参数）</param>
    /// <param name="compilationOptions">编译选项</param>
    /// <param name="previousProject">关联的前置项目（可选）</param>
    /// <returns>项目实例</returns>
    protected virtual Project CreateProject(Solution solution, DocumentCreationArgs args, CompilationOptions compilationOptions, Project? previousProject = null)
    {
        var name = args.Name ?? "New";
        var path = Path.Combine(args.WorkingDirectory, name);
        var id = ProjectId.CreateNewId(name);

        var parseOptions = ParseOptions.WithKind(args.SourceCodeKind);
        var isScript = args.SourceCodeKind == SourceCodeKind.Script;

        if (isScript)
        {
            compilationOptions = compilationOptions.WithScriptClassName(name);
        }

        var analyzerConfigDocuments = AnalyzerConfigFiles.Where(File.Exists).Select(file => DocumentInfo.Create(
            DocumentId.CreateNewId(id, debugName: file),
            name: file,
            loader: new FileTextLoader(file, defaultEncoding: null),
            filePath: file));

        solution = solution.AddProject(ProjectInfo.Create(
            id,
            VersionStamp.Create(),
            name,
            name,
            LanguageNames.CSharp,
            filePath: path,
            isSubmission: isScript,
            parseOptions: parseOptions,
            compilationOptions: compilationOptions,
            metadataReferences: previousProject != null ? [] : DefaultReferences,
            projectReferences: previousProject != null ? new[] { new ProjectReference(previousProject.Id) } : null)
            .WithAnalyzerConfigDocuments(analyzerConfigDocuments));

        var project = solution.GetProject(id)!;

        if (!isScript && GetUsings(project) is { Length: > 0 } usings)
        {
            project = project.AddDocument("RoslynPadGeneratedUsings", usings).Project;
        }

        return project;

        /// <summary>
        /// 生成项目的全局命名空间导入代码
        /// </summary>
        /// <param name="project">目标项目</param>
        /// <returns>全局导入代码字符串</returns>
        static string GetUsings(Project project)
        {
            if (project.CompilationOptions is CSharpCompilationOptions options)
            {
                return string.Join(" ", options.Usings.Select(i => $"global using {i};"));
            }

            return string.Empty;
        }
    }
}

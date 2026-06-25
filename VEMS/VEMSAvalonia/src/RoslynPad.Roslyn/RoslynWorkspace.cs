// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;

namespace RoslynPad.Roslyn;

/// <summary>
/// 封装 Roslyn 工作区的自定义实现，管理文档打开、解决方案变更等操作
/// </summary>
/// <param name="hostServices">宿主服务实例</param>
/// <param name="workspaceKind">工作区类型（默认：Host）</param>
/// <param name="roslynHost">关联的 RoslynHost 实例</param>
public class RoslynWorkspace(HostServices hostServices, string workspaceKind = WorkspaceKind.Host, RoslynHost? roslynHost = null) : Workspace(hostServices, workspaceKind)
{
    /// <summary>
    /// 获取或设置当前打开的文档 ID
    /// </summary>
    public DocumentId? OpenDocumentId { get; private set; }

    /// <summary>
    /// 获取关联的 RoslynHost 实例
    /// </summary>
    public RoslynHost? RoslynHost { get; } = roslynHost;

    /// <summary>
    /// 设置当前解决方案并触发工作区变更事件
    /// </summary>
    /// <param name="solution">新的解决方案实例</param>
    public new void SetCurrentSolution(Solution solution)
    {
        var oldSolution = CurrentSolution;
        var newSolution = base.SetCurrentSolution(solution);
        RaiseWorkspaceChangedEventAsync(WorkspaceChangeKind.SolutionChanged, oldSolution, newSolution);
    }

    /// <summary>
    /// 获取一个值，指示工作区是否支持打开文档
    /// </summary>
    public override bool CanOpenDocuments => true;

    /// <summary>
    /// 确定工作区是否支持应用指定类型的变更
    /// </summary>
    /// <param name="feature">要检查的变更类型</param>
    /// <returns>是否支持该变更类型</returns>
    public override bool CanApplyChange(ApplyChangesKind feature)
    {
        return feature switch
        {
            ApplyChangesKind.ChangeDocument or ApplyChangesKind.ChangeDocumentInfo or ApplyChangesKind.AddMetadataReference or ApplyChangesKind.RemoveMetadataReference or ApplyChangesKind.AddAnalyzerReference or ApplyChangesKind.RemoveAnalyzerReference => true,
            _ => false,
        };
    }

    /// <summary>
    /// 打开指定文档并关联文本容器
    /// </summary>
    /// <param name="documentId">要打开的文档 ID</param>
    /// <param name="textContainer">文档的文本容器</param>
    public void OpenDocument(DocumentId documentId, SourceTextContainer textContainer)
    {
        OpenDocumentId = documentId;
        OnDocumentOpened(documentId, textContainer);
        OnDocumentContextUpdated(documentId);
    }

    /// <summary>
    /// 文本变更应用前触发的事件（参数：文档ID、新文本）
    /// </summary>
    public event Action<DocumentId, SourceText>? ApplyingTextChange;

    /// <summary>
    /// 释放工作区资源
    /// </summary>
    /// <param name="finalize">是否由终结器调用</param>
    protected override void Dispose(bool finalize)
    {
        base.Dispose(finalize);

        ApplyingTextChange = null;
    }

    /// <summary>
    /// 应用文档文本变更
    /// </summary>
    /// <param name="id">文档 ID</param>
    /// <param name="text">新的文本内容</param>
    protected override void ApplyDocumentTextChanged(DocumentId id, SourceText text)
    {
        if (OpenDocumentId != id)
        {
            return;
        }

        ApplyingTextChange?.Invoke(id, text);

        OnDocumentTextChanged(id, text, PreservationMode.PreserveIdentity);
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.PickMembers;

namespace RoslynPad.Roslyn.LanguageServices.PickMembers;

/// <summary>
/// 选择成员服务的实现，负责创建选择成员对话框并处理对话框的返回结果
/// </summary>
[ExportWorkspaceService(typeof(IPickMembersService), ServiceLayer.Host), Shared]
[method: ImportingConstructor]
internal class PickMembersService(ExportFactory<IPickMembersDialog> dialogFactory) : IPickMembersService
{
    private readonly ExportFactory<IPickMembersDialog> _dialogFactory = dialogFactory;

    /// <summary>
    /// 显示选择成员对话框，获取用户选择的成员和选项结果
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="members">待选择的成员符号数组</param>
    /// <param name="options">选择成员的配置选项数组（可选）</param>
    /// <param name="selectAll">是否默认选中所有成员（默认true）</param>
    /// <returns>包含用户选择结果的<see cref="PickMembersResult"/>，若取消则返回<see cref="PickMembersResult.Canceled"/></returns>
    public PickMembersResult PickMembers(
        string title, ImmutableArray<ISymbol> members, ImmutableArray<PickMembersOption> options = default, bool selectAll = true)
    {
        options = options.NullToEmpty();

        var viewModel = new PickMembersDialogViewModel(members, options);
        var dialog = _dialogFactory.CreateExport().Value;
        dialog.Title = title;
        dialog.ViewModel = viewModel;
        if (dialog.Show() == true)
        {
            return new PickMembersResult(
                viewModel.MemberContainers.Where(c => c.IsChecked)
                                          .Select(c => c.MemberSymbol)
                                          .ToImmutableArray(),
                options, selectAll);
        }
        else
        {
            return PickMembersResult.Canceled;
        }
    }
}

/// <summary>
/// 选择成员对话框的接口，定义对话框的核心属性
/// </summary>
internal interface IPickMembersDialog : IRoslynDialog
{
    /// <summary>
    /// 获取或设置对话框的标题
    /// </summary>
    string? Title { get; set; }
}

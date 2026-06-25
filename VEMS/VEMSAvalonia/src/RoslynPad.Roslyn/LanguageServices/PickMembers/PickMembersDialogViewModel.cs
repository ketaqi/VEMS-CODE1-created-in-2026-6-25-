// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PickMembers;

namespace RoslynPad.Roslyn.LanguageServices.PickMembers;

/// <summary>
/// 选择成员对话框的视图模型，负责管理成员列表、选项列表、选中状态及成员排序等逻辑
/// </summary>
internal class PickMembersDialogViewModel : NotificationObject
{
    /// <summary>
    /// 获取或设置成员符号视图模型列表，每个项对应一个可选择的代码成员
    /// </summary>
    public List<MemberSymbolViewModel> MemberContainers { get; set; }

    /// <summary>
    /// 获取或设置选项视图模型列表，每个项对应一个选择成员的配置选项
    /// </summary>
    public List<OptionViewModel> Options { get; set; }

    /// <summary>
    /// 初始化 <see cref="PickMembersDialogViewModel"/> 类的新实例
    /// </summary>
    /// <param name="members">待选择的成员符号数组</param>
    /// <param name="options">选择成员的配置选项数组</param>
    internal PickMembersDialogViewModel(
        ImmutableArray<ISymbol> members,
        ImmutableArray<PickMembersOption> options)
    {
        MemberContainers = members.Select(m => new MemberSymbolViewModel(m)).ToList();
        Options = options.Select(o => new OptionViewModel(o)).ToList();
    }

    /// <summary>
    /// 取消所有成员的选中状态
    /// </summary>
    internal void DeselectAll()
    {
        foreach (var memberContainer in MemberContainers)
        {
            memberContainer.IsChecked = false;
        }
    }

    /// <summary>
    /// 选中所有成员
    /// </summary>
    internal void SelectAll()
    {
        foreach (var memberContainer in MemberContainers)
        {
            memberContainer.IsChecked = true;
        }
    }

    private int? _selectedIndex;

    /// <summary>
    /// 获取或设置当前选中成员的索引
    /// </summary>
    /// <remarks>设置时会触发相关属性（如<see cref="CanMoveUp"/>、<see cref="CanMoveDown"/>）的变更通知</remarks>
    public int? SelectedIndex
    {
        get => _selectedIndex;

        set
        {
            var newSelectedIndex = value == -1 ? null : value;
            if (newSelectedIndex == _selectedIndex)
            {
                return;
            }

            _selectedIndex = newSelectedIndex;

            OnPropertyChanged(nameof(CanMoveUp));
            OnPropertyChanged(nameof(MoveUpAutomationText));
            OnPropertyChanged(nameof(CanMoveDown));
            OnPropertyChanged(nameof(MoveDownAutomationText));
        }
    }

    /// <summary>
    /// 获取向上移动选中成员的自动化辅助文本
    /// </summary>
    /// <remarks>供辅助功能（如屏幕阅读器）使用，描述向上移动操作的含义</remarks>
    public string MoveUpAutomationText
    {
        get
        {
            if (!CanMoveUp || SelectedIndex == null)
            {
                return string.Empty;
            }

            return $"Move {MemberContainers[SelectedIndex.Value].MemberAutomationText} below {MemberContainers[SelectedIndex.Value - 1].MemberAutomationText}";
        }
    }

    /// <summary>
    /// 获取向下移动选中成员的自动化辅助文本
    /// </summary>
    /// <remarks>供辅助功能（如屏幕阅读器）使用，描述向下移动操作的含义</remarks>
    public string MoveDownAutomationText
    {
        get
        {
            if (!CanMoveDown || SelectedIndex == null)
            {
                return string.Empty;
            }

            return $"Move {MemberContainers[SelectedIndex.Value].MemberAutomationText} below {MemberContainers[SelectedIndex.Value + 1].MemberAutomationText}";
        }
    }

    /// <summary>
    /// 获取一个值，指示当前选中的成员是否可以向上移动
    /// </summary>
    public bool CanMoveUp
    {
        get
        {
            if (!SelectedIndex.HasValue)
            {
                return false;
            }

            var index = SelectedIndex.Value;
            return index > 0;
        }
    }

    /// <summary>
    /// 获取一个值，指示当前选中的成员是否可以向下移动
    /// </summary>
    public bool CanMoveDown
    {
        get
        {
            if (!SelectedIndex.HasValue)
            {
                return false;
            }

            var index = SelectedIndex.Value;
            return index < MemberContainers.Count - 1;
        }
    }

    /// <summary>
    /// 将选中的成员向上移动一位
    /// </summary>
    /// <remarks>调用前应确保<see cref="CanMoveUp"/>为true</remarks>
    internal void MoveUp()
    {
        Debug.Assert(CanMoveUp);
        if (SelectedIndex == null) return;

        var index = SelectedIndex.Value;
        Move(MemberContainers, index, delta: -1);
    }

    /// <summary>
    /// 将选中的成员向下移动一位
    /// </summary>
    /// <remarks>调用前应确保<see cref="CanMoveDown"/>为true</remarks>
    internal void MoveDown()
    {
        Debug.Assert(CanMoveDown);
        if (SelectedIndex == null) return;

        var index = SelectedIndex.Value;
        Move(MemberContainers, index, delta: 1);
    }

    /// <summary>
    /// 在列表中移动指定索引的项
    /// </summary>
    /// <param name="list">待操作的列表</param>
    /// <param name="index">要移动的项的当前索引</param>
    /// <param name="delta">移动的偏移量（-1向上，1向下）</param>
    private void Move(List<MemberSymbolViewModel> list, int index, int delta)
    {
        var param = list[index];
        list.RemoveAt(index);
        list.Insert(index + delta, param);

        SelectedIndex += delta;
    }

    /// <summary>
    /// 成员符号的视图模型，封装单个成员的显示、选中状态等逻辑
    /// </summary>
    /// <param name="symbol">对应的代码成员符号</param>
    internal class MemberSymbolViewModel(ISymbol symbol) : NotificationObject
    {
        /// <summary>
        /// 获取对应的代码成员符号
        /// </summary>
        public ISymbol MemberSymbol { get; } = symbol;

        /// <summary>
        /// 成员显示格式，定义成员名称的展示规则
        /// </summary>
        private static readonly SymbolDisplayFormat s_memberDisplayFormat = new(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeOptionalBrackets,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        private bool _isChecked = true;

        /// <summary>
        /// 获取或设置一个值，指示该成员是否被选中
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set { SetProperty(ref _isChecked, value); }
        }

        /// <summary>
        /// 获取成员的显示名称（按<see cref="s_memberDisplayFormat"/>格式化）
        /// </summary>
        public string MemberName => MemberSymbol.ToDisplayString(s_memberDisplayFormat);

        /// <summary>
        /// 获取成员对应的图标
        /// </summary>
        public Completion.Glyph Glyph => MemberSymbol.GetGlyph();

        /// <summary>
        /// 获取成员的自动化辅助文本（供辅助功能使用）
        /// </summary>
        public string MemberAutomationText => MemberSymbol.Kind + " " + MemberName;
    }

    /// <summary>
    /// 选项视图模型，封装单个配置选项的显示、选中状态等逻辑
    /// </summary>
    internal class OptionViewModel : NotificationObject
    {
        /// <summary>
        /// 获取对应的配置选项
        /// </summary>
        public PickMembersOption Option { get; }

        /// <summary>
        /// 获取选项的显示标题
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 初始化 <see cref="OptionViewModel"/> 类的新实例
        /// </summary>
        /// <param name="option">对应的配置选项</param>
        public OptionViewModel(PickMembersOption option)
        {
            Option = option;
            Title = option.Title;
            IsChecked = option.Value;
        }

        private bool _isChecked;

        /// <summary>
        /// 获取或设置一个值，指示该选项是否被选中
        /// </summary>
        /// <remarks>设置时会同步更新<see cref="Option"/>的Value属性</remarks>
        public bool IsChecked
        {
            get => _isChecked;

            set
            {
                Option.Value = value;
                SetProperty(ref _isChecked, value);
            }
        }

        /// <summary>
        /// 获取选项的自动化辅助文本（供辅助功能使用）
        /// </summary>
        public string MemberAutomationText => Option.Title;
    }
}

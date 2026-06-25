// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ChangeSignature;

namespace RoslynPad.Roslyn.LanguageServices.ChangeSignature;

/// <summary>
/// 更改签名对话框的视图模型，负责管理参数配置、UI状态和签名展示逻辑
/// </summary>
internal class ChangeSignatureDialogViewModel : NotificationObject
{
    /// <summary>
    /// 原始的参数配置信息
    /// </summary>
    private readonly ParameterConfiguration _originalParameterConfiguration;

    /// <summary>
    /// 表示this参数的视图模型（扩展方法的this参数）
    /// </summary>
    private readonly ParameterViewModel? _thisParameter;

    /// <summary>
    /// 无默认值的参数视图模型列表
    /// </summary>
    private readonly List<ParameterViewModel> _parametersWithoutDefaultValues;

    /// <summary>
    /// 有默认值的参数视图模型列表
    /// </summary>
    private readonly List<ParameterViewModel> _parametersWithDefaultValues;

    /// <summary>
    /// 表示params参数的视图模型
    /// </summary>
    private readonly ParameterViewModel? _paramsParameter;

    /// <summary>
    /// 禁用的参数视图模型集合（无法编辑/删除）
    /// </summary>
    private readonly HashSet<ParameterViewModel> _disabledParameters = [];

    /// <summary>
    /// 符号声明的展示部分（用于签名展示）
    /// </summary>
    private readonly ImmutableArray<SymbolDisplayPart> _declarationParts;

    /// <summary>
    /// 初始化 <see cref="ChangeSignatureDialogViewModel"/> 类的新实例
    /// </summary>
    /// <param name="parameters">参数配置信息</param>
    /// <param name="symbol">要更改签名的符号（方法/函数等）</param>
    internal ChangeSignatureDialogViewModel(ParameterConfiguration parameters, ISymbol symbol)
    {
        _originalParameterConfiguration = parameters;

        int startingSelectedIndex = 0;

        if (parameters.ThisParameter != null)
        {
            startingSelectedIndex++;

            _thisParameter = new ParameterViewModel(this, parameters.ThisParameter);
            _disabledParameters.Add(_thisParameter);
        }

        if (parameters.ParamsParameter != null)
        {
            _paramsParameter = new ParameterViewModel(this, parameters.ParamsParameter);
        }

        _declarationParts = symbol.ToDisplayParts(s_symbolDeclarationDisplayFormat);

        _parametersWithoutDefaultValues = parameters.ParametersWithoutDefaultValues.OfType<ExistingParameter>().Select(p => new ParameterViewModel(this, p)).ToList();
        _parametersWithDefaultValues = parameters.RemainingEditableParameters.OfType<ExistingParameter>().Select(p => new ParameterViewModel(this, p)).ToList();
        SelectedIndex = startingSelectedIndex;
    }

    /// <summary>
    /// 获取初始选中的索引位置
    /// </summary>
    /// <returns>初始选中索引</returns>
    public int GetStartingSelectionIndex()
    {
        return _thisParameter == null ? 0 : 1;
    }

    /// <summary>
    /// 获取或设置是否预览更改
    /// </summary>
    public bool PreviewChanges { get; set; }

    /// <summary>
    /// 获取是否可以移除当前选中的参数
    /// </summary>
    public bool CanRemove
    {
        get
        {
            if (!SelectedIndex.HasValue)
            {
                return false;
            }

            var index = SelectedIndex.Value;

            if (index == 0 && _thisParameter != null)
            {
                return false;
            }

            return !AllParameters[index].IsRemoved;
        }
    }

    /// <summary>
    /// 获取是否可以恢复当前选中的参数
    /// </summary>
    public bool CanRestore
    {
        get
        {
            if (!SelectedIndex.HasValue)
            {
                return false;
            }

            var index = SelectedIndex.Value;

            if (index == 0 && _thisParameter != null)
            {
                return false;
            }

            return AllParameters[index].IsRemoved;
        }
    }

    /// <summary>
    /// 移除当前选中的参数
    /// </summary>
    internal void Remove()
    {
        if (_selectedIndex == null) return;
        AllParameters[_selectedIndex.Value].IsRemoved = true;
        OnPropertyChanged(nameof(AllParameters));
        OnPropertyChanged(nameof(SignatureDisplay));
        OnPropertyChanged(nameof(IsOkButtonEnabled));
        OnPropertyChanged(nameof(CanRemove));
        OnPropertyChanged(nameof(CanRestore));
    }

    /// <summary>
    /// 恢复当前选中的已移除参数
    /// </summary>
    internal void Restore()
    {
        if (_selectedIndex == null) return;
        AllParameters[_selectedIndex.Value].IsRemoved = false;
        OnPropertyChanged(nameof(AllParameters));
        OnPropertyChanged(nameof(SignatureDisplay));
        OnPropertyChanged(nameof(IsOkButtonEnabled));
        OnPropertyChanged(nameof(CanRemove));
        OnPropertyChanged(nameof(CanRestore));
    }

    /// <summary>
    /// 获取当前编辑后的参数配置信息
    /// </summary>
    /// <returns>更新后的参数配置</returns>
    internal ParameterConfiguration GetParameterConfiguration()
    {
        return new ParameterConfiguration(
            _originalParameterConfiguration.ThisParameter,
            _parametersWithoutDefaultValues.Where(p => !p.IsRemoved).Select(p => (Parameter)new ExistingParameter(p.ParameterSymbol)).ToImmutableArray(),
            _parametersWithDefaultValues.Where(p => !p.IsRemoved).Select(p => (Parameter)new ExistingParameter(p.ParameterSymbol)).ToImmutableArray(),
            (_paramsParameter == null || _paramsParameter.IsRemoved) ? null : new ExistingParameter(_paramsParameter.ParameterSymbol),
            selectedIndex: -1);
    }

    /// <summary>
    /// 符号声明的展示格式配置
    /// </summary>
    private static readonly SymbolDisplayFormat s_symbolDeclarationDisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes,
        extensionMethodStyle: SymbolDisplayExtensionMethodStyle.StaticMethod,
        memberOptions:
            SymbolDisplayMemberOptions.IncludeType |
            SymbolDisplayMemberOptions.IncludeExplicitInterface |
            SymbolDisplayMemberOptions.IncludeAccessibility |
            SymbolDisplayMemberOptions.IncludeModifiers);

    /// <summary>
    /// 参数的展示格式配置
    /// </summary>
    private static readonly SymbolDisplayFormat s_parameterDisplayFormat = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes,
        parameterOptions:
            SymbolDisplayParameterOptions.IncludeType |
            SymbolDisplayParameterOptions.IncludeParamsRefOut |
            SymbolDisplayParameterOptions.IncludeDefaultValue |
            SymbolDisplayParameterOptions.IncludeExtensionThis |
            SymbolDisplayParameterOptions.IncludeName);

    /// <summary>
    /// 获取签名展示的标记文本（用于UI展示）
    /// </summary>
    public ImmutableArray<TaggedText> SignatureDisplay
    {
        get
        {
            // TODO: Should probably use original syntax & formatting exactly instead of regenerating here
            var displayParts = GetSignatureDisplayParts();

            return displayParts.ToTaggedText();
        }
    }

    /// <summary>
    /// 测试用：获取签名展示的文本内容
    /// </summary>
    /// <returns>签名文本</returns>
    internal string TEST_GetSignatureDisplayText()
    {
        return string.Concat(GetSignatureDisplayParts().Select(p => p.ToString()));
    }

    /// <summary>
    /// 获取签名展示的符号部分列表
    /// </summary>
    /// <returns>符号展示部分列表</returns>
    private List<SymbolDisplayPart> GetSignatureDisplayParts()
    {
        var displayParts = new List<SymbolDisplayPart>();

        displayParts.AddRange(_declarationParts);
        displayParts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, "("));

        bool first = true;
        foreach (var parameter in AllParameters.Where(p => !p.IsRemoved))
        {
            if (!first)
            {
                displayParts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, ","));
                displayParts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Space, null, " "));
            }

            first = false;
            displayParts.AddRange(parameter.ParameterSymbol.ToDisplayParts(s_parameterDisplayFormat));
        }

        displayParts.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Punctuation, null, ")"));
        return displayParts;
    }

    /// <summary>
    /// 获取所有参数的视图模型列表（包含this、无默认值、有默认值、params参数）
    /// </summary>
    public List<ParameterViewModel> AllParameters
    {
        get
        {
            var list = new List<ParameterViewModel>();
            if (_thisParameter != null)
            {
                list.Add(_thisParameter);
            }

            list.AddRange(_parametersWithoutDefaultValues);
            list.AddRange(_parametersWithDefaultValues);

            if (_paramsParameter != null)
            {
                list.Add(_paramsParameter);
            }

            return list;
        }
    }

    /// <summary>
    /// 获取是否可以将当前选中的参数上移
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
            index = _thisParameter == null ? index : index - 1;
            if (index <= 0 || index == _parametersWithoutDefaultValues.Count || index >= _parametersWithoutDefaultValues.Count + _parametersWithDefaultValues.Count)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 获取是否可以将当前选中的参数下移
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
            index = _thisParameter == null ? index : index - 1;
            if (index < 0 || index == _parametersWithoutDefaultValues.Count - 1 || index >= _parametersWithoutDefaultValues.Count + _parametersWithDefaultValues.Count - 1)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 将当前选中的参数上移
    /// </summary>
    internal void MoveUp()
    {
        Debug.Assert(CanMoveUp);
        if (SelectedIndex == null) return;

        var index = SelectedIndex.Value;
        index = _thisParameter == null ? index : index - 1;
        Move(index < _parametersWithoutDefaultValues.Count ? _parametersWithoutDefaultValues : _parametersWithDefaultValues, index < _parametersWithoutDefaultValues.Count ? index : index - _parametersWithoutDefaultValues.Count, -1);
    }

    /// <summary>
    /// 将当前选中的参数下移
    /// </summary>
    internal void MoveDown()
    {
        Debug.Assert(CanMoveDown);
        if (SelectedIndex == null) return;

        var index = SelectedIndex.Value;
        index = _thisParameter == null ? index : index - 1;
        Move(index < _parametersWithoutDefaultValues.Count ? _parametersWithoutDefaultValues : _parametersWithDefaultValues, index < _parametersWithoutDefaultValues.Count ? index : index - _parametersWithoutDefaultValues.Count, 1);
    }

    /// <summary>
    /// 移动参数在列表中的位置
    /// </summary>
    /// <param name="list">参数列表</param>
    /// <param name="index">当前参数索引</param>
    /// <param name="delta">移动偏移量（-1上移，1下移）</param>
    private void Move(List<ParameterViewModel> list, int index, int delta)
    {
        var param = list[index];
        list.RemoveAt(index);
        list.Insert(index + delta, param);

        SelectedIndex += delta;

        OnPropertyChanged(nameof(AllParameters));
        OnPropertyChanged(nameof(SignatureDisplay));
        OnPropertyChanged(nameof(IsOkButtonEnabled));
    }

    /// <summary>
    /// 尝试提交更改（验证是否可以确认）
    /// </summary>
    /// <returns>是否可以提交</returns>
    internal bool TrySubmit()
    {
        return IsOkButtonEnabled;
    }

    /// <summary>
    /// 判断参数是否被禁用（无法编辑）
    /// </summary>
    /// <param name="parameterViewModel">参数视图模型</param>
    /// <returns>是否禁用</returns>
    private bool IsDisabled(ParameterViewModel parameterViewModel)
    {
        return _disabledParameters.Contains(parameterViewModel);
    }

    /// <summary>
    /// 获取当前选中参数所属的分组列表
    /// </summary>
    /// <returns>参数分组列表</returns>
    private List<ParameterViewModel> GetSelectedGroup()
    {
        var index = SelectedIndex;
        index = _thisParameter == null ? index : index - 1;
        return index < _parametersWithoutDefaultValues.Count ? _parametersWithoutDefaultValues : index < _parametersWithoutDefaultValues.Count + _parametersWithDefaultValues.Count ? _parametersWithDefaultValues : [];
    }

    /// <summary>
    /// 获取确定按钮是否可用（是否有参数变更）
    /// </summary>
    public bool IsOkButtonEnabled
    {
        get
        {
            return AllParameters.Any(p => p.IsRemoved) ||
                !_parametersWithoutDefaultValues.Select(p => p.Parameter).SequenceEqual(_originalParameterConfiguration.ParametersWithoutDefaultValues) ||
                !_parametersWithDefaultValues.Select(p => p.Parameter).SequenceEqual(_originalParameterConfiguration.RemainingEditableParameters);
        }
    }

    /// <summary>
    /// 选中的参数索引
    /// </summary>
    private int? _selectedIndex;

    /// <summary>
    /// 获取或设置选中的参数索引
    /// </summary>
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
            OnPropertyChanged(nameof(CanMoveDown));
            OnPropertyChanged(nameof(CanRemove));
            OnPropertyChanged(nameof(CanRestore));
        }
    }

    /// <summary>
    /// 参数视图模型，封装单个参数的展示和状态
    /// </summary>
    public class ParameterViewModel(ChangeSignatureDialogViewModel changeSignatureDialogViewModel, ExistingParameter parameter)
    {
        /// <summary>
        /// 所属的更改签名对话框视图模型
        /// </summary>
        private readonly ChangeSignatureDialogViewModel _changeSignatureDialogViewModel = changeSignatureDialogViewModel;

        /// <summary>
        /// 获取原始参数信息
        /// </summary>
        public ExistingParameter Parameter { get; } = parameter;

        /// <summary>
        /// 获取参数符号
        /// </summary>
        public IParameterSymbol ParameterSymbol => Parameter.Symbol;

        /// <summary>
        /// 获取参数修饰符（out/ref/params/this等）
        /// </summary>
        public string Modifier
        {
            get
            {
                // Todo: support VB
                switch (ParameterSymbol.RefKind)
                {
                    case RefKind.Out:
                        return "out";
                    case RefKind.Ref:
                        return "ref";
                }

                if (ParameterSymbol.IsParams)
                {
                    return "params";
                }

                if (_changeSignatureDialogViewModel._thisParameter != null &&
                    SymbolEqualityComparer.Default.Equals(ParameterSymbol, _changeSignatureDialogViewModel._thisParameter.ParameterSymbol))
                {
                    return "this";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 获取参数类型的展示文本
        /// </summary>
        public string Type => ParameterSymbol.Type.ToDisplayString(s_parameterDisplayFormat);

        /// <summary>
        /// 获取参数名称
        /// </summary>
        public string ParameterName => ParameterSymbol.Name;

        /// <summary>
        /// 获取参数默认值的展示文本
        /// </summary>
        public string Default
        {
            get
            {
                if (!ParameterSymbol.HasExplicitDefaultValue)
                {
                    return string.Empty;
                }

                return ParameterSymbol.ExplicitDefaultValue == null
                    ? "null"
                    : ParameterSymbol.ExplicitDefaultValue is string
                        ? "\"" + ParameterSymbol.ExplicitDefaultValue + "\""
                        : ParameterSymbol.ExplicitDefaultValue.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// 获取参数是否被禁用
        /// </summary>
        public bool IsDisabled => _changeSignatureDialogViewModel.IsDisabled(this);

        /// <summary>
        /// 获取是否需要显示底部边框（用于UI分组展示）
        /// </summary>
        public bool NeedsBottomBorder
        {
            get
            {
                if (this == _changeSignatureDialogViewModel._thisParameter)
                {
                    return true;
                }

                if (this == _changeSignatureDialogViewModel._parametersWithoutDefaultValues.LastOrDefault() &&
                    (_changeSignatureDialogViewModel._parametersWithDefaultValues.Count != 0 || _changeSignatureDialogViewModel._paramsParameter != null))
                {
                    return true;
                }

                if (this == _changeSignatureDialogViewModel._parametersWithDefaultValues.LastOrDefault() &&
                    _changeSignatureDialogViewModel._paramsParameter != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 获取或设置参数是否被移除
        /// </summary>
        public bool IsRemoved { get; set; }
    }
}

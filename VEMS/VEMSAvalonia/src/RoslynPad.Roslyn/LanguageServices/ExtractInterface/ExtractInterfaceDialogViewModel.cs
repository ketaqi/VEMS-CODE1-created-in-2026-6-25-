// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Glyph = RoslynPad.Roslyn.Completion.Glyph;

namespace RoslynPad.Roslyn.LanguageServices.ExtractInterface;

/// <summary>
/// 接口存放位置枚举
/// </summary>
internal enum InterfaceDestination
{
    /// <summary>
    /// 当前文件
    /// </summary>
    CurrentFile,
    /// <summary>
    /// 新文件
    /// </summary>
    NewFile
}

/// <summary>
/// 提取接口对话框的视图模型，负责封装界面交互数据和验证逻辑
/// </summary>
internal class ExtractInterfaceDialogViewModel : NotificationObject
{
    /// <summary>
    /// 语法事实服务对象，用于标识符合法性校验
    /// </summary>
    private readonly object _syntaxFactsService;
    /// <summary>
    /// 冲突的类型名称集合
    /// </summary>
    private readonly ImmutableArray<string> _conflictingTypeNames;
    /// <summary>
    /// 默认命名空间
    /// </summary>
    private readonly string _defaultNamespace;
    /// <summary>
    /// 生成名称的类型参数后缀
    /// </summary>
    private readonly string _generatedNameTypeParameterSuffix;
    /// <summary>
    /// 语言名称（如C#）
    /// </summary>
    private readonly string _languageName;
    /// <summary>
    /// 文件扩展名（默认.cs）
    /// </summary>
    private readonly string _fileExtension;

    /// <summary>
    /// 初始化提取接口对话框视图模型
    /// </summary>
    /// <param name="syntaxFactsService">语法事实服务对象</param>
    /// <param name="defaultInterfaceName">默认接口名称</param>
    /// <param name="extractableMembers">可提取为接口成员的符号集合</param>
    /// <param name="conflictingTypeNames">冲突的类型名称集合</param>
    /// <param name="defaultNamespace">默认命名空间</param>
    /// <param name="generatedNameTypeParameterSuffix">生成名称的类型参数后缀</param>
    /// <param name="languageName">语言名称</param>
    internal ExtractInterfaceDialogViewModel(
        object syntaxFactsService,
        string defaultInterfaceName,
        ImmutableArray<ISymbol> extractableMembers,
        ImmutableArray<string> conflictingTypeNames,
        string defaultNamespace,
        string generatedNameTypeParameterSuffix,
        string languageName)
    {
        _syntaxFactsService = syntaxFactsService;
        _interfaceName = defaultInterfaceName;
        _conflictingTypeNames = conflictingTypeNames;
        _fileExtension = ".cs";
        _fileName = $"{defaultInterfaceName}.{_fileExtension}";
        _defaultNamespace = defaultNamespace;
        _generatedNameTypeParameterSuffix = generatedNameTypeParameterSuffix;
        _languageName = languageName;

        // 将可提取成员转换为视图模型并按名称排序
        MemberContainers = [.. extractableMembers.Select(m => new MemberSymbolViewModel(m)).OrderBy(s => s.MemberName)];
    }

    /// <summary>
    /// 尝试提交提取接口的配置，执行数据验证
    /// </summary>
    /// <returns>验证通过返回true，否则返回false</returns>
    internal bool TrySubmit()
    {
        var trimmedInterfaceName = InterfaceName.Trim();
        var trimmedFileName = FileName.Trim();

        // 验证：至少选择一个成员
        if (!MemberContainers.Any(c => c.IsChecked))
        {
            SendFailureNotification("YouMustSelectAtLeastOneMember");
            return false;
        }

        // 验证：接口名称不与现有类型冲突
        if (_conflictingTypeNames.Contains(trimmedInterfaceName))
        {
            SendFailureNotification("InterfaceNameConflictsWithTypeName");
            return false;
        }

        // 注释的代码：验证接口名称是否为合法标识符
        //if (!_syntaxFactsService.IsValidIdentifier(trimmedInterfaceName))
        //{
        //    SendFailureNotification($"InterfaceNameIsNotAValidIdentifier {_languageName}");
        //    return false;
        //}

        // 验证：文件名不含非法字符
        if (trimmedFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            SendFailureNotification("IllegalCharactersInPath");
            return false;
        }

        // 验证：文件扩展名符合要求
        if (!Path.GetExtension(trimmedFileName).Equals(_fileExtension, StringComparison.OrdinalIgnoreCase))
        {
            SendFailureNotification($"FileNameMustHaveTheExtension {_fileExtension}");
            return false;
        }

        // 待实现：处理文件名已存在的情况
        // TODO: Deal with filename already existing

        return true;
    }

    /// <summary>
    /// 发送验证失败的通知
    /// </summary>
    /// <param name="message">通知消息标识</param>
    private void SendFailureNotification(string message)
    {
        // 注释的代码：通知服务发送提示
        //_notificationService.SendNotification(message, severity: NotificationSeverity.Information);
    }

    /// <summary>
    /// 取消选择所有成员
    /// </summary>
    internal void DeselectAll()
    {
        foreach (var memberContainer in MemberContainers)
        {
            memberContainer.IsChecked = false;
        }
    }

    /// <summary>
    /// 全选所有成员
    /// </summary>
    internal void SelectAll()
    {
        foreach (var memberContainer in MemberContainers)
        {
            memberContainer.IsChecked = true;
        }
    }

    /// <summary>
    /// 可提取的成员视图模型集合
    /// </summary>
    public List<MemberSymbolViewModel> MemberContainers { get; set; }

    /// <summary>
    /// 接口名称私有字段
    /// </summary>
    private string _interfaceName;
    /// <summary>
    /// 接口名称（双向绑定属性）
    /// </summary>
    public string InterfaceName
    {
        get => _interfaceName;
        set
        {
            if (SetProperty(ref _interfaceName, value))
            {
                // 接口名称变更时自动更新文件名
                FileName = $"{value.Trim()}{_fileExtension}";
                OnPropertyChanged(nameof(GeneratedName));
            }
        }
    }

    /// <summary>
    /// 生成的完整接口名称（包含命名空间和类型参数后缀）
    /// </summary>
    public string GeneratedName =>
        $"{(string.IsNullOrEmpty(_defaultNamespace) ? string.Empty : _defaultNamespace + ".")}{_interfaceName.Trim()}{_generatedNameTypeParameterSuffix}";

    /// <summary>
    /// 接口存放位置私有字段
    /// </summary>
    private InterfaceDestination _destination = InterfaceDestination.NewFile;
    /// <summary>
    /// 接口存放位置（双向绑定属性）
    /// </summary>
    public InterfaceDestination Destination
    {
        get { return _destination; }
        set
        {
            if (SetProperty(ref _destination, value))
            {
                // 存放位置变更时触发文件名输入框启用状态更新
                OnPropertyChanged(nameof(FileNameEnabled));
            }
        }
    }

    /// <summary>
    /// 文件名输入框是否启用（仅当选择新文件时启用）
    /// </summary>
    public bool FileNameEnabled => Destination == InterfaceDestination.NewFile;

    /// <summary>
    /// 文件名私有字段
    /// </summary>
    private string _fileName;
    /// <summary>
    /// 文件名（双向绑定属性）
    /// </summary>
    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    /// <summary>
    /// 成员符号视图模型，封装单个可提取成员的显示和选择状态
    /// </summary>
    internal class MemberSymbolViewModel(ISymbol symbol) : NotificationObject
    {
        /// <summary>
        /// 成员符号对象
        /// </summary>
        public ISymbol MemberSymbol { get; } = symbol;

        /// <summary>
        /// 成员显示格式配置
        /// </summary>
        private static readonly SymbolDisplayFormat s_memberDisplayFormat = new(
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeOptionalBrackets,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        /// <summary>
        /// 成员是否被选中私有字段
        /// </summary>
        private bool _isChecked = true;
        /// <summary>
        /// 成员是否被选中（双向绑定属性）
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        /// <summary>
        /// 成员显示名称（格式化后的符号名称）
        /// </summary>
        public string MemberName => MemberSymbol.ToDisplayString(s_memberDisplayFormat);

        /// <summary>
        /// 成员对应的图标
        /// </summary>
        public Glyph Glyph => MemberSymbol.GetGlyph();
    }
}

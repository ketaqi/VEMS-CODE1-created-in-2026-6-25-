using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ExtractInterface;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace RoslynPad.Roslyn.LanguageServices.ExtractInterface;

/// <summary>
/// 提取接口选项服务实现类，负责处理提取接口的选项配置与对话框交互
/// </summary>
[ExportWorkspaceService(typeof(IExtractInterfaceOptionsService))]
[method: ImportingConstructor]
internal sealed class ExtractInterfaceOptionsService(ExportFactory<IExtractInterfaceDialog> dialogFactory) : IExtractInterfaceOptionsService
{
    /// <summary>
    /// 提取接口对话框的导出工厂，用于创建对话框实例
    /// </summary>
    private readonly ExportFactory<IExtractInterfaceDialog> _dialogFactory = dialogFactory;

    /// <summary>
    /// 获取提取接口的配置选项
    /// </summary>
    /// <param name="document">当前操作的文档对象</param>
    /// <param name="extractableMembers">可提取为接口成员的符号集合</param>
    /// <param name="defaultInterfaceName">默认的接口名称</param>
    /// <param name="conflictingTypeNames">冲突的类型名称集合</param>
    /// <param name="defaultNamespace">默认的命名空间</param>
    /// <param name="generatedNameTypeParameterSuffix">生成名称的类型参数后缀</param>
    /// <returns>提取接口的选项结果，包含用户配置或取消状态</returns>
    public ExtractInterfaceOptionsResult GetExtractInterfaceOptions(Document document, ImmutableArray<ISymbol> extractableMembers, string defaultInterfaceName, ImmutableArray<string> conflictingTypeNames, string defaultNamespace, string generatedNameTypeParameterSuffix)
    {
        // 创建视图模型，封装提取接口的配置数据
        var viewModel = new ExtractInterfaceDialogViewModel(
            document.GetRequiredLanguageService<ISyntaxFactsService>(),
            defaultInterfaceName,
            extractableMembers,
            conflictingTypeNames,
            defaultNamespace,
            generatedNameTypeParameterSuffix,
            document.Project.Language);

        // 创建对话框实例并绑定视图模型
        var dialog = _dialogFactory.CreateExport().Value;
        dialog.ViewModel = viewModel;

        // 根据对话框交互结果构建返回值
        var options = dialog.Show() == true
            ? new ExtractInterfaceOptionsResult(
                isCancelled: false,
                includedMembers: viewModel.MemberContainers.Where(c => c.IsChecked).Select(c => c.MemberSymbol).AsImmutable(),
                interfaceName: viewModel.InterfaceName.Trim(),
                fileName: viewModel.FileName.Trim(),
                location: GetLocation(viewModel.Destination))
            : ExtractInterfaceOptionsResult.Cancelled;
        return options;
    }

    /// <summary>
    /// 将界面选择的目标位置转换为提取接口结果对应的位置枚举
    /// </summary>
    /// <param name="destination">界面选择的接口存放位置</param>
    /// <returns>提取接口结果的位置枚举值</returns>
    /// <exception cref="InvalidOperationException">未知的目标位置枚举值时抛出</exception>
    private static ExtractInterfaceOptionsResult.ExtractLocation GetLocation(InterfaceDestination destination) => destination switch
    {
        InterfaceDestination.CurrentFile => ExtractInterfaceOptionsResult.ExtractLocation.SameFile,
        InterfaceDestination.NewFile => ExtractInterfaceOptionsResult.ExtractLocation.NewFile,
        _ => throw new InvalidOperationException($"不支持的接口目标位置：{destination}"),
    };
}

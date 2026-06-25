using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ChangeSignature;
using Microsoft.CodeAnalysis.Host.Mef;

namespace RoslynPad.Roslyn.LanguageServices.ChangeSignature;

/// <summary>
/// 更改签名选项服务，用于弹出更改签名对话框并获取用户配置
/// </summary>
[ExportWorkspaceService(typeof(IChangeSignatureOptionsService))]
[method: ImportingConstructor]
internal sealed class ChangeSignatureOptionsService(ExportFactory<IChangeSignatureDialog> dialogFactory) : IChangeSignatureOptionsService
{
    /// <summary>
    /// 更改签名对话框的导出工厂
    /// </summary>
    private readonly ExportFactory<IChangeSignatureDialog> _dialogFactory = dialogFactory;

    /// <summary>
    /// 获取更改签名的用户配置选项
    /// </summary>
    /// <param name="document">语义文档</param>
    /// <param name="positionForTypeBinding">类型绑定的位置</param>
    /// <param name="symbol">要更改签名的符号</param>
    /// <param name="parameters">初始参数配置</param>
    /// <returns>更改签名选项结果，取消则返回null</returns>
    public ChangeSignatureOptionsResult? GetChangeSignatureOptions(SemanticDocument document, int positionForTypeBinding, ISymbol symbol, Microsoft.CodeAnalysis.ChangeSignature.ParameterConfiguration parameters)
    {
        var viewModel = new ChangeSignatureDialogViewModel(new ParameterConfiguration(parameters), symbol);

        var dialog = _dialogFactory.CreateExport().Value;
        dialog.ViewModel = viewModel;
        var result = dialog.Show();

        return result == true
            ? new ChangeSignatureOptionsResult(new SignatureChange(new ParameterConfiguration(parameters), viewModel.GetParameterConfiguration()).ToInternal(), previewChanges: false)
            : null;
    }
}

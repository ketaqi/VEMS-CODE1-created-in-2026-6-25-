namespace RoslynPad.Roslyn.LanguageServices.ChangeSignature;

/// <summary>
/// 签名更改信息，包含原始配置和更新后的配置
/// </summary>
internal sealed class SignatureChange(ParameterConfiguration originalConfiguration, ParameterConfiguration updatedConfiguration)
{
    /// <summary>
    /// 获取原始参数配置
    /// </summary>
    public ParameterConfiguration OriginalConfiguration { get; } = originalConfiguration;

    /// <summary>
    /// 获取更新后的参数配置
    /// </summary>
    public ParameterConfiguration UpdatedConfiguration { get; } = updatedConfiguration;

    /// <summary>
    /// 转换为Roslyn内部的签名更改类型
    /// </summary>
    /// <returns>Roslyn内部签名更改对象</returns>
    internal Microsoft.CodeAnalysis.ChangeSignature.SignatureChange ToInternal()
    {
        return new Microsoft.CodeAnalysis.ChangeSignature.SignatureChange(OriginalConfiguration.ToInternal(), UpdatedConfiguration.ToInternal());
    }
}

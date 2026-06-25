using System.Collections.Immutable;
using Microsoft.CodeAnalysis.ChangeSignature;

namespace RoslynPad.Roslyn.LanguageServices.ChangeSignature;

/// <summary>
/// 参数配置类，封装更改签名所需的所有参数信息
/// </summary>
internal sealed class ParameterConfiguration
{
    /// <summary>
    /// Roslyn内部的参数配置对象
    /// </summary>
    private readonly Microsoft.CodeAnalysis.ChangeSignature.ParameterConfiguration? _inner;

    /// <summary>
    /// 从Roslyn内部参数配置初始化 <see cref="ParameterConfiguration"/> 实例
    /// </summary>
    /// <param name="inner">Roslyn内部参数配置</param>
    public ParameterConfiguration(Microsoft.CodeAnalysis.ChangeSignature.ParameterConfiguration inner)
    {
        _inner = inner;
        ThisParameter = inner.ThisParameter;
        ParametersWithoutDefaultValues = inner.ParametersWithoutDefaultValues;
        RemainingEditableParameters = inner.RemainingEditableParameters;
        ParamsParameter = inner.ParamsParameter;
        SelectedIndex = inner.SelectedIndex;
    }

    /// <summary>
    /// 初始化 <see cref="ParameterConfiguration"/> 实例
    /// </summary>
    /// <param name="thisParameter">this参数（扩展方法）</param>
    /// <param name="parametersWithoutDefaultValues">无默认值的参数列表</param>
    /// <param name="remainingEditableParameters">剩余可编辑参数（有默认值）</param>
    /// <param name="paramsParameter">params参数</param>
    /// <param name="selectedIndex">选中的参数索引</param>
    public ParameterConfiguration(ExistingParameter? thisParameter, ImmutableArray<Parameter> parametersWithoutDefaultValues, ImmutableArray<Parameter> remainingEditableParameters, ExistingParameter? paramsParameter, int selectedIndex)
    {
        ThisParameter = thisParameter;
        ParametersWithoutDefaultValues = parametersWithoutDefaultValues;
        RemainingEditableParameters = remainingEditableParameters;
        ParamsParameter = paramsParameter;
        SelectedIndex = selectedIndex;
    }

    /// <summary>
    /// 获取扩展方法的this参数
    /// </summary>
    public ExistingParameter? ThisParameter { get; }

    /// <summary>
    /// 获取无默认值的参数列表
    /// </summary>
    public ImmutableArray<Parameter> ParametersWithoutDefaultValues { get; }

    /// <summary>
    /// 获取剩余可编辑参数列表（有默认值）
    /// </summary>
    public ImmutableArray<Parameter> RemainingEditableParameters { get; }

    /// <summary>
    /// 获取params参数
    /// </summary>
    public ExistingParameter? ParamsParameter { get; }

    /// <summary>
    /// 获取或设置选中的参数索引
    /// </summary>
    public int SelectedIndex { get; set; }

    /// <summary>
    /// 转换为Roslyn内部的参数配置类型
    /// </summary>
    /// <returns>Roslyn内部参数配置对象</returns>
    internal Microsoft.CodeAnalysis.ChangeSignature.ParameterConfiguration ToInternal()
    {
        return _inner ??
               new Microsoft.CodeAnalysis.ChangeSignature.ParameterConfiguration(ThisParameter,
                   ParametersWithoutDefaultValues, RemainingEditableParameters, ParamsParameter, SelectedIndex);
    }
}

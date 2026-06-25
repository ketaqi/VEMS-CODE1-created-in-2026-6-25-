using System;
using OpticalChainSimulator.Models;

namespace OpticalChainSimulator.Services.ComponentServices;

/// <summary>
/// 光学元件工厂接口
/// 定义光学元件模型（Model）和视图模型（ViewModel）的创建契约，
/// 用于标准化不同类型光学元件的实例化流程
/// </summary>
public interface IOpticalElementFactory
{
    /// <summary>
    /// 创建带有默认参数配置的光学元件模型实例
    /// </summary>
    /// <returns>默认参数配置的 OpticalElement 模型实例，包含元件基础属性和默认值</returns>
    OpticalElement CreateDefaultModel();

    /// <summary>
    /// 根据已有光学元件模型创建对应的视图模型（ViewModel）
    /// </summary>
    /// <param name="model">待绑定的光学元件模型实例</param>
    /// <returns>自定义的 OpticalElementViewModel 实例；若返回 null，主程序将使用默认的视图模型实现</returns>
    OpticalChainSimulator.ViewModels.OpticalElementViewModel? CreateViewModel(OpticalElement model);
}
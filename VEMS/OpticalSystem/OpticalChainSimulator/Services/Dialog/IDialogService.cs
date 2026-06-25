using System;
using System.Threading.Tasks;

namespace OpticalChainSimulator.Services.Dialog;

/// <summary>
/// 对话框服务接口
/// 定义应用中对话框的核心操作契约，统一管理对话框的显示逻辑
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// 显示通用模态对话框
    /// 模态对话框会阻塞当前窗口的交互，直到对话框关闭
    /// </summary>
    /// <typeparam name="TViewModel">对话框绑定的视图模型类型，限制为引用类型</typeparam>
    /// <param name="viewModel">对话框对应的视图模型实例，承载对话框的业务数据和交互逻辑</param>
    /// <returns>异步任务，等待任务完成表示对话框已关闭，无返回值（原注释中“返回的值”因方法无返回泛型，此处按实际语义注释）</returns>
    Task ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class;

    /// <summary>
    /// 显示关于对话框（关于本应用的信息弹窗）
    /// 专用的关于对话框，简化泛型复杂度，无需传入视图模型参数
    /// </summary>
    /// <returns>异步任务，等待任务完成表示关于对话框已关闭</returns>
    Task ShowAboutDialogAsync();
}
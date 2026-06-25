using System;
using System.Numerics;
using OpticalChainSimulator.Models;
using OpticalChainSimulator.Views;
using OpticalChainSimulator.yuanjianku;

namespace OpticalChainSimulator.Services.SceneService
{
    /// <summary>
    /// ISceneService 接口的 Avalonia 实现类
    /// 作为场景服务的具体实现，通过委托 MainView 的方法来完成光学元件的添加、删除、更新、查询等场景操作
    /// </summary>
    public class AvaloniaSceneService : ISceneService
    {
        /// <summary>
        /// 主视图实例，所有场景操作最终委托给该实例执行
        /// </summary>
        private readonly MainView _mainView;

        /// <summary>
        /// 初始化 AvaloniaSceneService 实例
        /// </summary>
        /// <param name="mainView">主视图实例（不能为空），用于委托执行场景操作</param>
        /// <exception cref="ArgumentNullException">当 mainView 为 null 时抛出该异常</exception>
        public AvaloniaSceneService(MainView mainView)
        {
            _mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
        }

        /// <summary>
        /// 清空场景中所有光学元件（委托 MainView 执行）
        /// </summary>
        /// <param name="updateScene">是否同步更新场景渲染（默认值：true）</param>
        public void ClearAll(bool updateScene = true)
            => _mainView.RemoveAllElements(updateScene);

        /// <summary>
        /// 添加光源元件到场景（委托 MainView 执行）
        /// </summary>
        /// <param name="template">光源元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void AddSource(OpticalElement template, bool notifyVm = false)
            => _mainView.AddSource(template, notifyVm);

        /// <summary>
        /// 添加反射镜1元件到场景（委托 MainView 执行）
        /// </summary>
        /// <param name="template">反射镜1元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void AddMirror1(OpticalElement1 template, bool notifyVm = false)
            => _mainView.AddMirror1(template, notifyVm);

        /// <summary>
        /// 添加反射镜元件到场景（委托 MainView 执行）
        /// </summary>
        /// <param name="template">反射镜元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void AddMirror(OpticalElement template, bool notifyVm = false)
            => _mainView.AddMirror(template, notifyVm);

        /// <summary>
        /// 添加透镜元件到场景（委托 MainView 执行）
        /// </summary>
        /// <param name="template">透镜元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void AddLens(OpticalElement template, bool notifyVm = false)
            => _mainView.AddLens(template, notifyVm);

        /// <summary>
        /// 添加探测器元件到场景（委托 MainView 执行）
        /// </summary>
        /// <param name="template">探测器元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void AddDet(OpticalElement template, bool notifyVm = false)
            => _mainView.AddDet(template, notifyVm);

        /// <summary>
        /// 根据元件ID移除场景中的光学元件（委托 MainView 执行）
        /// </summary>
        /// <param name="elementId">要移除的元件唯一标识</param>
        /// <param name="updateScene">是否同步更新场景渲染（默认值：true）</param>
        /// <returns>移除成功返回true，元件不存在或移除失败返回false</returns>
        public bool RemoveElementByElementId(Guid elementId, bool updateScene = true)
            => _mainView.RemoveElementByElementId(elementId, updateScene);

        /// <summary>
        /// 尝试根据元件ID获取其中心点三维坐标（委托 MainView 执行）
        /// </summary>
        /// <param name="elementId">元件唯一标识</param>
        /// <param name="center">输出参数：获取到的元件中心点坐标（Vector3表示三维空间坐标）</param>
        /// <returns>获取成功返回true，元件不存在或获取失败返回false</returns>
        public bool TryGetElementCenterByElementId(Guid elementId, out Vector3 center)
            => _mainView.TryGetElementCenterByElementId(elementId, out center);

        /// <summary>
        /// 刷新公共元件列表（委托 MainView 执行）
        /// </summary>
        /// <param name="selectLast">是否选中列表最后一项（参数语义由 MainView 定义）</param>
        public void RefreshListPublic(bool selectLast)
            => _mainView.RefreshListPublic(selectLast);

        /// <summary>
        /// 更新场景中指定的OpticalElement1类型光学元件（委托 MainView 执行）
        /// </summary>
        /// <param name="template">包含更新后属性的元件模板模型</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        public void UpdateElement(OpticalElement1 template, bool notifyVm = false)
         => _mainView.UpdateElement(template, notifyVm);

        /// <summary>
        /// 高亮显示场景中指定ID的光学元件（委托 MainView 执行）
        /// </summary>
        /// <param name="elementId">需要高亮的元件唯一标识</param>
        public void HighlightElement(Guid elementId)
        => _mainView.HighlightElement(elementId);
    }
}
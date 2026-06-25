using System;
using System.Numerics;
using OpticalChainSimulator.Models;
using OpticalChainSimulator.yuanjianku;

namespace OpticalChainSimulator.Services.SceneService
{
    /// <summary>
    /// 场景服务接口
    /// 定义视图模型（VM）所需的最低限度场景操作抽象，封装光学元件的核心操作逻辑
    /// </summary>
    public interface ISceneService
    {
        /// <summary>
        /// 清空场景中所有光学元件
        /// </summary>
        /// <param name="updateScene">是否同步更新场景渲染（默认值：true）</param>
        void ClearAll(bool updateScene = true);

        /// <summary>
        /// 添加光源元件到场景
        /// </summary>
        /// <param name="template">光源元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void AddSource(OpticalElement template, bool notifyVm = false);

        /// <summary>
        /// 添加反射镜1元件到场景
        /// </summary>
        /// <param name="template">反射镜1元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void AddMirror1(OpticalElement1 template, bool notifyVm = false);

        /// <summary>
        /// 添加反射镜元件到场景
        /// </summary>
        /// <param name="template">反射镜元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void AddMirror(OpticalElement template, bool notifyVm = false);

        /// <summary>
        /// 添加透镜元件到场景
        /// </summary>
        /// <param name="template">透镜元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void AddLens(OpticalElement template, bool notifyVm = false);

        /// <summary>
        /// 添加探测器元件到场景
        /// </summary>
        /// <param name="template">探测器元件的模板模型（包含元件基础属性）</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void AddDet(OpticalElement template, bool notifyVm = false);

        /// <summary>
        /// 根据元件ID移除场景中的光学元件
        /// </summary>
        /// <param name="elementId">要移除的元件唯一标识</param>
        /// <param name="updateScene">是否同步更新场景渲染（默认值：true）</param>
        /// <returns>移除成功返回true，元件不存在或移除失败返回false</returns>
        bool RemoveElementByElementId(Guid elementId, bool updateScene = true);

        /// <summary>
        /// 尝试根据元件ID获取其中心点三维坐标
        /// </summary>
        /// <param name="elementId">元件唯一标识</param>
        /// <param name="center">输出参数：获取到的元件中心点坐标（Vector3表示三维空间坐标）</param>
        /// <returns>获取成功返回true，元件不存在或获取失败返回false</returns>
        bool TryGetElementCenterByElementId(Guid elementId, out Vector3 center);

        /// <summary>
        /// 刷新公共元件列表
        /// </summary>
        /// <param name="something">触发列表刷新的状态/标识参数（参数语义由业务逻辑定义）</param>
        void RefreshListPublic(bool something);

        /// <summary>
        /// 更新场景中指定的OpticalElement1类型光学元件
        /// </summary>
        /// <param name="template">包含更新后属性的元件模板模型</param>
        /// <param name="notifyVm">是否通知视图模型更新状态（默认值：false）</param>
        void UpdateElement(OpticalElement1 template, bool notifyVm = false);

        /// <summary>
        /// 高亮显示场景中指定ID的光学元件
        /// </summary>
        /// <param name="elementId">需要高亮的元件唯一标识</param>
        void HighlightElement(Guid elementId);
    }
}
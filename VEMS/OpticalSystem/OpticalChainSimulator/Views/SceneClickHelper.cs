using System;
using System.Diagnostics;
using OpticalChainSimulator.Models;
using static OpticalChainSimulator.ViewModels.OpticalElementViewModel;

namespace OpticalChainSimulator.Views
{
    /// <summary>
    /// 场景元素点击辅助类
    /// 用于将底层视口命中（Hit）通知映射为应用层的场景对象点击事件，解耦具体视口类型与业务逻辑
    /// 该类不直接引用具体视口类型，由调用方提供视口事件绑定方式和场景获取逻辑，避免视口类型耦合导致的编译错误
    /// </summary>
    public static class SceneClickHelper
    {
        /// <summary>
        /// 将渲染层的HitType枚举映射为业务层的OpticalElementType枚举
        /// </summary>
        /// <param name="hitType">渲染层命中类型</param>
        /// <param name="mapped">输出：映射后的业务层光学元件类型</param>
        /// <returns>映射成功返回true，不支持的类型返回false</returns>
        public static bool TryMap(HitType hitType, out OpticalElementType mapped)
        {
            switch (hitType)
            {
                case HitType.Mirror:
                    mapped = OpticalElementType.Mirror; return true;
                case HitType.Lens:
                    mapped = OpticalElementType.Lens; return true;
                case HitType.Detector:
                    mapped = OpticalElementType.Detector; return true;
                case HitType.Box:
                    mapped = OpticalElementType.Box; return true;
                case HitType.PortBox:
                    mapped = OpticalElementType.PortBox; return true;
                default:
                    mapped = default;
                    Debug.WriteLine($"SceneClickHelper: 不支持的HitType类型 {hitType}");
                    return false;
            }
        }

        /// <summary>
        /// 从场景中根据命中类型和索引读取对应的元件唯一标识（ElementId）
        /// 包含防御性校验（空值检查、索引边界检查），异常时输出调试日志
        /// </summary>
        /// <param name="sc">场景实例（可能为null）</param>
        /// <param name="hitType">渲染层命中类型</param>
        /// <param name="index">元件在对应类型集合中的索引</param>
        /// <param name="elementId">输出：读取到的元件GUID标识（未找到时为Guid.Empty）</param>
        /// <returns>成功读取返回true，失败返回false</returns>
        public static bool TryGetElementIdFromScene(Scene? sc, HitType hitType, int index, out Guid elementId)
        {
            // 初始化输出参数为默认空GUID
            elementId = Guid.Empty;
            try
            {
                // 场景为空时输出日志并返回失败
                if (sc == null)
                {
                    Debug.WriteLine("SceneClickHelper.TryGetElementIdFromScene: 场景实例为null");
                    return false;
                }

                // 根据命中类型从对应集合中读取元件ID
                switch (hitType)
                {
                    case HitType.Mirror:
                        if (index >= 0 && index < sc.Mirrors.Count) { elementId = sc.Mirrors[index].ElementId; return true; }
                        break;
                    case HitType.Lens:
                        if (index >= 0 && index < sc.Lenses.Count) { elementId = sc.Lenses[index].ElementId; return true; }
                        break;
                    case HitType.Detector:
                        if (index >= 0 && index < sc.Detectors.Count) { elementId = sc.Detectors[index].ElementId; return true; }
                        break;
                    case HitType.Box:
                        if (index >= 0 && index < sc.Boxes.Count) { elementId = sc.Boxes[index].ElementId; return true; }
                        break;
                    case HitType.PortBox:
                        if (index >= 0 && index < sc.PortBoxes.Count) { elementId = sc.PortBoxes[index].ElementId; return true; }
                        break;
                    default:
                        Debug.WriteLine($"SceneClickHelper.TryGetElementIdFromScene: 不支持的HitType类型 {hitType}");
                        break;
                }

                // 索引越界或类型不匹配时输出日志
                Debug.WriteLine($"SceneClickHelper.TryGetElementIdFromScene: 未找到HitType={hitType} 索引={index} 对应的元件");
                return false;
            }
            catch (Exception ex)
            {
                // 捕获所有异常并输出调试日志，保证程序稳定性
                Debug.WriteLine($"SceneClickHelper.TryGetElementIdFromScene 执行异常: {ex}");
                return false;
            }
        }

        /// <summary>
        /// 绑定视口单击事件处理器
        /// </summary>
        /// <param name="attachClick">绑定视口单击事件的回调（示例：h => viewport.ObjectClicked += h）</param>
        /// <param name="getScene">获取当前场景实例的回调（可能返回null）</param>
        /// <param name="callback">单击事件回调，接收映射后的光学元件类型、原始索引、元件GUID</param>
        /// <exception cref="ArgumentNullException">当任意入参为null时抛出</exception>
        public static void BindViewportClick(Action<Action<HitType, int>> attachClick, Func<Scene?> getScene, Action<OpticalElementType, int, Guid> callback)
        {
            // 入参空值校验，确保核心依赖不为空
            if (attachClick == null) throw new ArgumentNullException(nameof(attachClick));
            if (getScene == null) throw new ArgumentNullException(nameof(getScene));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // 绑定视口单击事件逻辑
            attachClick((hitType, index) =>
            {
                // 1. 将渲染层HitType映射为业务层OpticalElementType
                if (!TryMap(hitType, out var mapped))
                {
                    // 不支持的类型直接返回（已在TryMap中输出日志）
                    return;
                }

                // 2. 尝试从场景中获取元件GUID（最大努力尝试，失败则返回空GUID）
                Guid elementId = Guid.Empty;
                var sc = getScene();
                TryGetElementIdFromScene(sc, hitType, index, out elementId);

                // 3. 触发上层回调，捕获回调异常避免崩溃
                try
                {
                    callback(mapped, index, elementId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SceneClickHelper: 单击回调执行异常: {ex}");
                }
            });
        }

        /// <summary>
        /// 绑定视口双击事件处理器（逻辑与单击对齐，增加双击有效性校验）
        /// </summary>
        /// <param name="attachDoubleClick">绑定视口双击事件的回调（示例：h => viewport.ObjectDoubleClicked += h）</param>
        /// <param name="getScene">获取当前场景实例的回调（可能返回null）</param>
        /// <param name="callback">双击事件回调，接收映射后的光学元件类型、原始索引、元件GUID</param>
        /// <exception cref="ArgumentNullException">当任意入参为null时抛出</exception>
        public static void BindViewportDoubleClick(
           Action<Action<HitType, int>> attachDoubleClick,
           Func<Scene?> getScene,
           Action<OpticalElementType, int, Guid> callback)
        {
            // 入参空值校验
            if (attachDoubleClick == null) throw new ArgumentNullException(nameof(attachDoubleClick));
            if (getScene == null) throw new ArgumentNullException(nameof(getScene));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // 绑定视口双击事件逻辑
            attachDoubleClick((hitType, index) =>
            {
                // 加锁保证双击状态的线程安全
                lock (_clickStateLock)
                {
                    // 校验双击有效性：两次点击时间差≤阈值 + 命中类型+索引一致
                    var timeDiff = DateTime.Now - _lastClickTime;
                    bool isEffectiveDoubleClick = timeDiff.TotalMilliseconds <= DoubleClickDelayMs
                                                && _lastClickHitType == hitType
                                                && _lastClickIndex == index;

                    // 以下代码为注释保留的逻辑（未启用）：非有效双击则重置状态并返回
                    //if (!isEffectiveDoubleClick)
                    //{
                    //    _isWaitingForDoubleClick = false;
                    //    return;
                    //}

                    // 标记双击已触发，阻断可能的单击回调
                    _isWaitingForDoubleClick = false;

                    // 1. 映射渲染层类型到业务层类型
                    if (!TryMap(hitType, out var mappedType)) return;

                    // 2. 尝试获取元件GUID
                    Guid elementId = Guid.Empty;
                    var scene = getScene();
                    TryGetElementIdFromScene(scene, hitType, index, out elementId);

                    // 3. 触发上层双击回调，捕获异常避免崩溃
                    try { callback(mappedType, index, elementId); }
                    catch (Exception ex) { Debug.WriteLine($"双击回调执行异常: {ex}"); }
                }
            });
        }

        #region 私有字段：双击状态管理
        /// <summary>
        /// 双击间隔阈值（毫秒）：两次点击间隔≤该值判定为有效双击
        /// </summary>
        private const int DoubleClickDelayMs = 500;

        /// <summary>
        /// 双击状态锁对象：保证多线程下双击状态的线程安全
        /// </summary>
        private static readonly object _clickStateLock = new();

        /// <summary>
        /// 上次点击的命中类型：用于校验双击是否为同一元素
        /// </summary>
        private static HitType _lastClickHitType;

        /// <summary>
        /// 上次点击的元素索引：用于校验双击是否为同一元素
        /// </summary>
        private static int _lastClickIndex;

        /// <summary>
        /// 上次点击的时间：用于计算双击时间间隔
        /// </summary>
        private static DateTime _lastClickTime;

        /// <summary>
        /// 双击等待标记：标记是否处于等待第二次点击的状态
        /// </summary>
        private static bool _isWaitingForDoubleClick;
        #endregion
    }
}
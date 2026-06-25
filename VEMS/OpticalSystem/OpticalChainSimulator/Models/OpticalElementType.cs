namespace OpticalChainSimulator.Models;

/// <summary>
/// 光学元件类型枚举
/// 定义光学链仿真系统中支持的所有光学元件类型，涵盖基础光学器件、组合器件及自定义几何体
/// </summary>
public enum OpticalElementType
{
    /// <summary>
    /// 光源
    /// </summary>
    Source,

    /// <summary>
    /// 反射镜
    /// </summary>
    Mirror,

    /// <summary>
    /// 透镜
    /// </summary>
    Lens,

    /// <summary>
    /// 自由空间段
    /// </summary>
    FreeSpace,

    /// <summary>
    /// 光阑
    /// </summary>
    Aperture,

    /// <summary>
    /// 滤光片
    /// </summary>
    Filter,

    /// <summary>
    /// 探测器
    /// </summary>
    Detector,

    /// <summary>
    /// 扩束系统（两透镜等效）
    /// </summary>
    BeamExpander,

    /// <summary>
    /// 分束器
    /// </summary>
    BeamSplitter,

    /// <summary>
    /// 箱体（自定义几何体，来自库示例/扩展类型）
    /// </summary>
    Box,

    /// <summary>
    /// 端口箱（自定义几何体，来自库示例/扩展类型）
    /// </summary>
    PortBox
}
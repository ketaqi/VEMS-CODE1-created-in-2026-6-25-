using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpticalChainSimulator.yuanjianku
{
    /// <summary>
    /// 光学元件基础类
    /// 封装光学元件的核心属性（标识、名称、类型、参数等），继承ViewModelBase以支持MVVM数据绑定
    /// </summary>
    public partial class OpticalElement1 : ViewModelBase
    {
        /// <summary>
        /// 元件的整型唯一标识符
        /// </summary>
        public int Id1 { get; set; }

        /// <summary>
        /// 元件的全局唯一标识符（GUID）
        /// 默认为自动生成的新GUID，确保元件标识的唯一性
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 元件在界面上显示的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 元件的图标标识
        /// 可使用Emoji、字体图标等字符串形式表示
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 光学元件的类型枚举（如光源、反射镜、透镜等）
        /// </summary>
        public OpticalElementType Type { get; set; }

        /// <summary>
        /// 元件的参数字典
        /// 存储参数名称（键）与对应数值（值）的键值对，默认初始化为空字典
        /// </summary>
        public Dictionary<string, double> Parameters1 { get; set; } = new();

        /// <summary>
        /// 标识元件在界面上是否处于展开状态
        /// 标记为ObservableProperty以支持MVVM双向绑定（CommunityToolkit.Mvvm特性）
        /// </summary>
        [ObservableProperty]
        private bool isExpanded;

        /// <summary>
        /// 主要参数摘要信息
        /// 根据不同的光学元件类型返回对应的摘要文本（当前暂统一返回"1"）
        /// </summary>
        public string MainParameterSummary
        {
            get
            {
                switch (Type)
                {
                    case OpticalElementType.Source:        // 光源类型
                        return "1";
                    case OpticalElementType.Mirror:       // 反射镜类型
                        return "1";
                    case OpticalElementType.Lens:         // 透镜类型
                        return "1";
                    case OpticalElementType.FreeSpace:    // 自由空间类型
                        return "1";
                    case OpticalElementType.Aperture:     // 光阑类型
                        return "1";
                    case OpticalElementType.Filter:       // 滤光片类型
                        return "1";
                    case OpticalElementType.Detector:     // 探测器类型
                        return "1";
                    case OpticalElementType.BeamExpander: // 扩束器类型
                        return "1";
                    default:                              // 未知类型
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// 元件的空间坐标点（三维向量）
        /// 表示元件在三维空间中的位置
        /// </summary>
        public Vector3 Point1;

        /// <summary>
        /// 元件的法向量（三维向量）
        /// 表示元件在三维空间中的朝向
        /// </summary>
        public Vector3 Normal;
    }
}
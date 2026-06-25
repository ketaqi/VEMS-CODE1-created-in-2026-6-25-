using System;
using System.ComponentModel;
using PropertyModels.ComponentModel;

namespace RoslynPad.FeatureDemos.Models
{
    /// <summary>
    /// 表示三维向量的可观察对象。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类继承自 <see cref="MiniReactiveObject"/>，支持属性变更通知，
    /// 可用于数据绑定场景。
    /// </para>
    /// <para>
    /// 使用 <see cref="ExpandableObjectConverter"/> 类型转换器，
    /// 可在属性网格中展开显示各分量。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var position = new Vector3(1.0, 2.0, 3.0);
    /// position.PropertyChanged += (s, e) => Console.WriteLine($"{e.PropertyName} 已更改");
    /// position.X = 5.0;  // 触发 PropertyChanged 事件
    /// Console.WriteLine(position);  // 输出: 5,2,3
    /// </code>
    /// </example>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [ExpandableObjectDisplayMode(IsCategoryVisible = NullableBooleanType.No)]
    public class Vector3 : MiniReactiveObject
    {
        /// <summary>
        /// X 分量的后备字段。
        /// </summary>
        private double _x;

        /// <summary>
        /// Y 分量的后备字段。
        /// </summary>
        private double _y;

        /// <summary>
        /// Z 分量的后备字段。
        /// </summary>
        private double _z;

        /// <summary>
        /// 获取或设置向量的 X 分量。
        /// </summary>
        /// <value>X 轴上的坐标值。</value>
        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        /// <summary>
        /// 获取或设置向量的 Y 分量。
        /// </summary>
        /// <value>Y 轴上的坐标值。</value>
        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        /// <summary>
        /// 获取或设置向量的 Z 分量。
        /// </summary>
        /// <value>Z 轴上的坐标值。</value>
        public double Z
        {
            get => _z;
            set => this.RaiseAndSetIfChanged(ref _z, value);
        }

        /// <summary>
        /// 初始化 <see cref="Vector3"/> 类的新实例，所有分量为零。
        /// </summary>
        public Vector3()
        {
        }

        /// <summary>
        /// 使用指定的分量值初始化 <see cref="Vector3"/> 类的新实例。
        /// </summary>
        /// <param name="x">X 分量的值。</param>
        /// <param name="y">Y 分量的值。</param>
        /// <param name="z">Z 分量的值。</param>
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// 返回表示当前向量的字符串。
        /// </summary>
        /// <returns>格式为 "X,Y,Z" 的字符串。</returns>
        public override string ToString() => $"{X},{Y},{Z}";
    }
}

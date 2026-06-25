using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Platform;
using PropertyModels.Collections;
using PropertyModels.ComponentModel;

namespace RoslynPad.FeatureDemos.Models
{
    /// <summary>
    /// 表示三维向量的结构体（值类型）。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="Vector3"/> 类不同，此结构体为值类型，
    /// 适用于高性能场景或需要值语义的情况。
    /// </remarks>
    public struct SVector3
    {
#pragma warning disable CA1051 // 不要声明可见实例字段
        /// <summary>
        /// X 分量。
        /// </summary>
        public float x;

        /// <summary>
        /// Y 分量。
        /// </summary>
        public float y;

        /// <summary>
        /// Z 分量。
        /// </summary>
        public float z;
#pragma warning restore CA1051 // 不要声明可见实例字段

        /// <summary>
        /// 返回表示当前向量的字符串。
        /// </summary>
        /// <returns>格式为 "x, y, z" 的字符串，各分量保留一位小数。</returns>
        public readonly override string ToString() => $"{x:0.0}, {y:0.0}, {z: 0.0}";
    }

    /// <summary>
    /// 演示扩展属性功能的测试对象。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类展示了属性网格中多种类型的属性支持，包括：
    /// <list type="bullet">
    ///   <item><description>结构体属性（值类型和引用类型的向量）</description></item>
    ///   <item><description>可选择列表（国家/地区选择器）</description></item>
    ///   <item><description>布尔属性（开关、三态等）</description></item>
    ///   <item><description>数值属性（带范围限制和自定义操作）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class TestExtendsObject : MiniReactiveObject
    {
        /// <summary>
        /// 获取或设置三维向量对象（引用类型）。
        /// </summary>
        /// <value>可展开编辑的 <see cref="Vector3"/> 实例。</value>
        [Category("Struct")]
        public Vector3 vec3Object { get; set; } = new();

        /// <summary>
        /// 获取或设置三维向量结构体（值类型）。
        /// </summary>
        /// <value><see cref="SVector3"/> 结构体实例。</value>
        [Category("Struct")]
        public SVector3 vec3Struct { get; set; }

        /// <summary>
        /// 获取或设置三维向量结构体的绑定列表。
        /// </summary>
        /// <value>支持数据绑定的 <see cref="SVector3"/> 列表。</value>
        [Category("Struct")]
        public BindingList<SVector3> vec3BindingList { get; set; } =
        [
            new() { x = 7.8f, y = 3.14f, z = 0.0f }
        ];

        /// <summary>
        /// 获取或设置国家/地区可选择列表。
        /// </summary>
        /// <value>包含国旗图标的国家信息选择列表。</value>
        [Category("SelectableList")]
        public SelectableList<CountryInfo> Countries { get; set; }

        /// <summary>
        /// 获取或设置开关状态。
        /// </summary>
        /// <value>开关的当前状态，默认为 <see langword="true"/>。</value>
        [Category("Boolean")]
        public bool toggleAble { get; set; } = true;

        /// <summary>
        /// 获取或设置禁用状态。
        /// </summary>
        /// <value>禁用标志，默认为 <see langword="false"/>。</value>
        [Category("Boolean")]
        public bool disableAble { get; set; } = false;

        /// <summary>
        /// 获取或设置三态布尔值。
        /// </summary>
        /// <value>
        /// <see langword="true"/>、<see langword="false"/> 或 <see langword="null"/>（不确定状态）。
        /// </value>
        [Category("Boolean")]
        public bool? threeState { get; set; }

        /// <summary>
        /// 获取或设置只读布尔值。
        /// </summary>
        /// <value>只读的布尔状态。</value>
        [Category("Boolean")]
        [ReadOnly(true)]
        public bool readonlyBoolean { get; set; }

        /// <summary>
        /// 获取只读的静态布尔属性。
        /// </summary>
        /// <value>始终返回 <see langword="true"/>。</value>
        [Category("Boolean")]
        public static bool readonlyBoolean2 => true;

        /// <summary>
        /// 获取或设置自定义标签的布尔值。
        /// </summary>
        /// <value>布尔值状态。</value>
        [Category("Boolean")]
        public bool customLabel { get; set; }

        /// <summary>
        /// 获取或设置无自定义操作的数值。
        /// </summary>
        /// <value>范围在 0 到 1024 之间的整数。</value>
        [Category("Numeric")]
        [Range(0, 1024)]
        [PropertyOperationVisibility(PropertyOperationVisibility.Visible)]
        public int NoCustomOperationNumber { get; set; }

        /// <summary>
        /// 获取或设置带自定义菜单操作的数值。
        /// </summary>
        /// <value>范围在 0 到 1024 之间的整数。</value>
        [Category("Numeric")]
        [Range(0, 1024)]
        [PropertyOperationVisibility(PropertyOperationVisibility.Visible)]
        public int CustomOperationMenuNumber { get; set; }

        /// <summary>
        /// 获取或设置带自定义控件操作的数值。
        /// </summary>
        /// <value>范围在 0 到 1024 之间的整数。</value>
        [Category("Numeric")]
        [Range(0, 1024)]
        [PropertyOperationVisibility(PropertyOperationVisibility.Visible)]
        public int CustomOperationControlNumber { get; set; }

        /// <summary>
        /// 初始化 <see cref="TestExtendsObject"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// 构造函数从程序集资源中加载国家/地区标志图片，
        /// 并初始化 <see cref="Countries"/> 列表，默认选中中国。
        /// </remarks>
        public TestExtendsObject()
        {
            List<CountryInfo> list = [];

            var assets = AssetLoader.GetAssets(new Uri($"avares://{GetType().Assembly.GetName().Name}/Assets/country-flags"), null);
            foreach (var asset in assets)
            {
                list.Add(new CountryInfo(asset));
            }

            Countries = new SelectableList<CountryInfo>(list, list.Find(x => x.Code == "cn") ?? list.FirstOrDefault()!);
        }
    }

    /// <summary>
    /// 表示国家/地区信息，包含国旗图像。
    /// </summary>
    /// <remarks>
    /// 此类从嵌入资源加载国旗图片，并尝试使用 <see cref="RegionInfo"/>
    /// 获取国家/地区的本地化显示名称。
    /// </remarks>
    public class CountryInfo
    {
        /// <summary>
        /// 获取或设置国旗图像。
        /// </summary>
        /// <value>国家/地区的旗帜图像。</value>
        public Avalonia.Media.IImage Flag { get; set; }

        /// <summary>
        /// 获取或设置国家/地区名称。
        /// </summary>
        /// <value>本地化的国家/地区显示名称。</value>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置国家/地区代码。
        /// </summary>
        /// <value>ISO 3166-1 alpha-2 国家/地区代码（如 "cn"、"us"）。</value>
        public string Code { get; set; }

        /// <summary>
        /// 使用指定的资源 URI 初始化 <see cref="CountryInfo"/> 类的新实例。
        /// </summary>
        /// <param name="asset">国旗图片的资源 URI。</param>
        /// <remarks>
        /// 从资源路径中提取国家代码，并尝试通过 <see cref="RegionInfo"/>
        /// 获取本地化名称。若无法获取，则使用国家代码作为名称。
        /// </remarks>
        public CountryInfo(Uri asset)
        {
            Name = Code = Path.GetFileNameWithoutExtension(asset.LocalPath);

            try
            {
                var regionInfo = new RegionInfo(Code);
                Name = regionInfo.DisplayName;
            }
            catch
            {
                // 若无法识别国家代码，保留原始代码作为名称
            }

            using var stream = AssetLoader.Open(asset);
            Flag = new Avalonia.Media.Imaging.Bitmap(stream);
        }

        /// <summary>
        /// 返回国家/地区的名称。
        /// </summary>
        /// <returns>国家/地区的显示名称。</returns>
        public override string ToString() => Name;

        /// <summary>
        /// 确定指定对象是否等于当前国家信息。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>若国家代码相同则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public override bool Equals(object? obj)
        {
            if (obj is CountryInfo ci)
            {
                return Code == ci.Code;
            }

            return false;
        }

        /// <summary>
        /// 获取当前国家信息的哈希代码。
        /// </summary>
        /// <returns>基于国家代码的哈希代码。</returns>
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Code.GetHashCode();
    }
}

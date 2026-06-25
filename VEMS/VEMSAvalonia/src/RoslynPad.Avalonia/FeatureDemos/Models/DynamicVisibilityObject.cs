using System.ComponentModel;
using PropertyModels.ComponentModel;
using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;

namespace RoslynPad.FeatureDemos.Models
{
    /// <summary>
    /// 演示动态属性可见性功能的对象。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类展示了如何使用 <see cref="PropertyVisibilityConditionAttribute"/> 
    /// 和 <see cref="ConditionTargetAttribute"/> 实现属性的条件可见性。
    /// </para>
    /// <para>
    /// 属性的可见性可以基于：
    /// <list type="bullet">
    ///   <item><description>布尔属性的值（如 <see cref="IsShowPath"/>）</description></item>
    ///   <item><description>枚举属性的值（如 <see cref="Platform"/>）</description></item>
    ///   <item><description>多个属性的组合条件（如 <see cref="IsShowUnixLoginInfo"/>）</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class DynamicVisibilityObject : ReactiveObject
    {
        /// <summary>
        /// 获取或设置是否显示路径属性。
        /// </summary>
        /// <value>
        /// <see langword="true"/> 表示显示 <see cref="Path"/> 属性；
        /// <see langword="false"/> 表示隐藏。默认为 <see langword="true"/>。
        /// </value>
        /// <remarks>
        /// 此属性标记为 <see cref="ConditionTargetAttribute"/>，
        /// 可作为其他属性可见性条件的依据。
        /// </remarks>
        [ConditionTarget]
        public bool IsShowPath { get; set; } = true;

        /// <summary>
        /// 获取或设置文件路径。
        /// </summary>
        /// <value>文件路径字符串，默认为空字符串。</value>
        /// <remarks>
        /// <para>
        /// 仅当 <see cref="IsShowPath"/> 为 <see langword="true"/> 时可见。
        /// </para>
        /// <para>
        /// 使用 <see cref="PathBrowsableAttribute"/> 指定文件筛选器，
        /// 支持的格式包括：jpg、png、bmp、tag。
        /// </para>
        /// </remarks>
        [PropertyVisibilityCondition(nameof(IsShowPath), true)]
        [PathBrowsable(Filters = "Image Files(*.jpg;*.png;*.bmp;*.tag)|*.jpg;*.png;*.bmp;*.tag")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置当前平台类型。
        /// </summary>
        /// <value>平台标识符，默认为 <see cref="PlatformID.Win32NT"/>。</value>
        /// <remarks>
        /// 此属性标记为 <see cref="ConditionTargetAttribute"/>，
        /// 用于控制平台相关属性的可见性。
        /// </remarks>
        [ConditionTarget]
        public PlatformID Platform { get; set; } = PlatformID.Win32NT;

        /// <summary>
        /// 获取或设置 Unix 版本信息。
        /// </summary>
        /// <value>Unix 系统版本字符串，默认为空字符串。</value>
        /// <remarks>
        /// 仅当 <see cref="Platform"/> 为 <see cref="PlatformID.Unix"/> 时可见。
        /// </remarks>
        [PropertyVisibilityCondition(nameof(Platform), PlatformID.Unix)]
        [ConditionTarget]
        public string UnixVersion { get; set; } = string.Empty;

        /// <summary>
        /// 获取一个值，指示是否应显示 Unix 登录信息。
        /// </summary>
        /// <value>
        /// 当同时满足以下条件时返回 <see langword="true"/>：
        /// <list type="bullet">
        ///   <item><description><see cref="IsShowPath"/> 为 <see langword="true"/></description></item>
        ///   <item><description><see cref="Platform"/> 为 <see cref="PlatformID.Unix"/></description></item>
        ///   <item><description><see cref="UnixVersion"/> 不为空</description></item>
        /// </list>
        /// </value>
        /// <remarks>
        /// <para>
        /// 此属性演示了复杂的多条件可见性控制。
        /// </para>
        /// <para>
        /// 使用 <see cref="DependsOnPropertyAttribute"/> 标记依赖的属性，
        /// 以便在依赖属性变更时自动重新计算。
        /// </para>
        /// </remarks>
        [Browsable(false)]
        [DependsOnProperty(nameof(IsShowPath), nameof(Platform), nameof(UnixVersion))]
        [ConditionTarget]
        public bool IsShowUnixLoginInfo => IsShowPath && Platform == PlatformID.Unix && UnixVersion.IsNotNullOrEmpty();

        /// <summary>
        /// 获取或设置 Unix 登录信息。
        /// </summary>
        /// <value>包含用户名和密码的登录信息对象。</value>
        /// <remarks>
        /// 仅当 <see cref="IsShowUnixLoginInfo"/> 为 <see langword="true"/> 时可见。
        /// 使用 <see cref="ExpandableObjectConverter"/> 在属性网格中展开显示。
        /// </remarks>
        [PropertyVisibilityCondition(nameof(IsShowUnixLoginInfo), true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        // ReSharper disable once InconsistentNaming
        public LoginInfo unixLogInInfo { get; set; } = new();
    }
}

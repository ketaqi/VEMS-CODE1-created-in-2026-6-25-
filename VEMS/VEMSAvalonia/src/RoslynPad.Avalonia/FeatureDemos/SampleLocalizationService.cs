using Avalonia.PropertyGrid.Localization;
using RoslynPad.UI;

namespace RoslynPad.FeatureDemos
{
    /// <summary>
    /// 功能演示模块的示例本地化服务。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类继承自 <see cref="AssemblyJsonAssetLocalizationService1"/>，
    /// 用于从当前程序集的嵌入式 JSON 资源中加载本地化字符串。
    /// </para>
    /// <para>
    /// 本地化资源文件应放置在程序集的 Assets 文件夹中，
    /// 文件命名格式通常为：<c>Localization.{culture}.json</c>（如 <c>Localization.zh-CN.json</c>）。
    /// </para>
    /// <para>
    /// 此服务主要用于属性网格控件的界面文本本地化，包括：
    /// <list type="bullet">
    ///   <item><description>属性名称和描述的翻译</description></item>
    ///   <item><description>分类名称的翻译</description></item>
    ///   <item><description>按钮和菜单文本的翻译</description></item>
    ///   <item><description>验证消息的翻译</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 注册本地化服务：
    /// <code language="csharp">
    /// // 在应用程序启动时注册
    /// LocalizationService.Default = new SampleLocalizationService();
    /// 
    /// // 切换语言
    /// LocalizationService.Default.Culture = new CultureInfo("zh-CN");
    /// </code>
    /// 
    /// JSON 资源文件示例（Localization.zh-CN.json）：
    /// <code language="json">
    /// {
    ///   "On": "开",
    ///   "Off": "关",
    ///   "Min": "最小值",
    ///   "Max": "最大值",
    ///   "Operation": "操作",
    ///   "GenError": "生成错误"
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssemblyJsonAssetLocalizationService1"/>
    /// <seealso cref="LocalizationService"/>
    internal class SampleLocalizationService : AssemblyJsonAssetLocalizationService1
    {
        /// <summary>
        /// 初始化 <see cref="SampleLocalizationService"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// 构造函数将当前类所在的程序集传递给基类，
        /// 基类将从该程序集中加载嵌入的本地化 JSON 资源文件。
        /// </remarks>
        public SampleLocalizationService()
            : base(typeof(SampleLocalizationService).Assembly)
        {
        }
    }
}

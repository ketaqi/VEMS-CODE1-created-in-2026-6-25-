namespace RoslynPad.Models
{
    /// <summary>
    /// 应用程序快捷键配置数据模型。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此类存储应用程序中所有可配置快捷键的绑定值，
    /// 用于序列化/反序列化用户的快捷键偏好设置。
    /// </para>
    /// <para>
    /// 快捷键字符串格式通常为：<c>"Ctrl+S"</c>、<c>"Ctrl+Shift+N"</c> 等。
    /// </para>
    /// </remarks>
    public class ShortcutData
    {
        #region 初始设置

        /// <summary>
        /// 获取或设置初始语言设置（备用）。
        /// </summary>
        public required string InitialLanguage1 { get; set; }

        /// <summary>
        /// 获取或设置初始主题设置（备用）。
        /// </summary>
        public required string InitialTheme1 { get; set; }

        /// <summary>
        /// 获取或设置编辑器字体大小。
        /// </summary>
        public required int EditorFontSize { get; set; }

        /// <summary>
        /// 获取或设置输出窗口字体大小。
        /// </summary>
        public required int OutputFontSize { get; set; }

        /// <summary>
        /// 获取或设置编辑器字体系列（备用）。
        /// </summary>
        public required string EditorFontFamily1 { get; set; }

        /// <summary>
        /// 获取或设置编辑器字体系列。
        /// </summary>
        public required string EditorFontFamily { get; set; }

        /// <summary>
        /// 获取或设置初始编程语言。
        /// </summary>
        public required string InitialLanguage { get; set; }

        /// <summary>
        /// 获取或设置初始主题。
        /// </summary>
        public required string InitialTheme { get; set; }

        /// <summary>
        /// 获取或设置初始工作文件夹路径。
        /// </summary>
        public required string InitialFolderPath { get; set; }

        #endregion

        #region 文件菜单快捷键

        /// <summary>
        /// 获取或设置文件菜单快捷键。
        /// </summary>
        public required string FileMenu { get; set; }

        /// <summary>
        /// 获取或设置新建文件快捷键。
        /// </summary>
        public required string NewFileShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开文件快捷键。
        /// </summary>
        public required string OpenFileShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开文件夹快捷键。
        /// </summary>
        public required string OpenFolderShortcut { get; set; }

        /// <summary>
        /// 获取或设置默认文件夹快捷键。
        /// </summary>
        public required string DefaultFolderShortcut { get; set; }

        /// <summary>
        /// 获取或设置保存文件快捷键。
        /// </summary>
        public required string SaveFileShortcut { get; set; }

        /// <summary>
        /// 获取或设置另存为快捷键。
        /// </summary>
        public required string SaveAsShortcut { get; set; }

        /// <summary>
        /// 获取或设置保存全部快捷键。
        /// </summary>
        public required string SaveAllShortcut { get; set; }

        /// <summary>
        /// 获取或设置关闭标签页快捷键。
        /// </summary>
        public required string CloseTabShortcut { get; set; }

        /// <summary>
        /// 获取或设置关闭所有标签页快捷键。
        /// </summary>
        public required string CloseAllShortcut { get; set; }

        /// <summary>
        /// 获取或设置退出应用程序快捷键。
        /// </summary>
        public required string ExitAppShortcut { get; set; }

        #endregion

        #region 代码菜单快捷键

        /// <summary>
        /// 获取或设置代码菜单快捷键。
        /// </summary>
        public required string CodeMenu { get; set; }

        /// <summary>
        /// 获取或设置搜索文本快捷键。
        /// </summary>
        public required string SearchTextShortcut { get; set; }

        /// <summary>
        /// 获取或设置替换文本快捷键。
        /// </summary>
        public required string ReplaceTextShortcut { get; set; }

        /// <summary>
        /// 获取或设置格式化代码快捷键。
        /// </summary>
        public required string FormatCodeShortcut { get; set; }

        /// <summary>
        /// 获取或设置注释选中内容快捷键。
        /// </summary>
        public required string CommentSelectionShortcut { get; set; }

        /// <summary>
        /// 获取或设置取消注释选中内容快捷��。
        /// </summary>
        public required string UncommentSelectionShortcut { get; set; }

        /// <summary>
        /// 获取或设置调试代码快捷键。
        /// </summary>
        public required string DebugCodeShortcut { get; set; }

        /// <summary>
        /// 获取或设置运行代码快捷键。
        /// </summary>
        public required string RunCodeShortcut { get; set; }

        /// <summary>
        /// 获取或设置添加类快捷键。
        /// </summary>
        public required string AddClassShortcut { get; set; }

        /// <summary>
        /// 获取或设置添加方法快捷键。
        /// </summary>
        public required string AddMethodShortcut { get; set; }

        #endregion

        #region 输出菜单快捷键

        /// <summary>
        /// 获取或设置输出菜单快捷键。
        /// </summary>
        public required string OutputMenu { get; set; }

        /// <summary>
        /// 获取或设置复制消息快捷键。
        /// </summary>
        public required string CopyMessageShortcut { get; set; }

        /// <summary>
        /// 获取或设置清除消息快捷键。
        /// </summary>
        public required string ClearMessageShortcut { get; set; }

        /// <summary>
        /// 获取或设置最小化所有图形窗口快捷键。
        /// </summary>
        public required string AllFiguresMinimizedShortcut { get; set; }

        /// <summary>
        /// 获取或设置关闭所有图形窗口快捷键。
        /// </summary>
        public required string CloseAllFiguresShortcut { get; set; }

        #endregion

        #region 帮助菜单快捷键

        /// <summary>
        /// 获取或设置帮助菜单快捷键。
        /// </summary>
        public required string HelpMenu { get; set; }

        /// <summary>
        /// 获取或设置入门指南快捷键。
        /// </summary>
        public required string GetStartedShortcut { get; set; }

        /// <summary>
        /// 获取或设置 VEMS 文档快捷键。
        /// </summary>
        public required string VEMSDocsShortcut { get; set; }

        /// <summary>
        /// 获取或设置链接快捷键。
        /// </summary>
        public required string LinksShortcut { get; set; }

        /// <summary>
        /// 获取或设置帮助快捷键。
        /// </summary>
        public required string HelpShortcut { get; set; }

        /// <summary>
        /// 获取或设置许可证信息快捷键。
        /// </summary>
        public required string LicenseInfoShortcut { get; set; }

        /// <summary>
        /// 获取或设置更新应用程序快捷键。
        /// </summary>
        public required string UpdateAppShortcut { get; set; }

        /// <summary>
        /// 获取或设置关于 VEMS 快捷键。
        /// </summary>
        public required string AboutVEMSShortcut { get; set; }

        /// <summary>
        /// 获取或设置联系我们快捷键。
        /// </summary>
        public required string ContactUSShortcut { get; set; }

        /// <summary>
        /// 获取或设置加入社区快捷键。
        /// </summary>
        public required string JoinShortcut { get; set; }

        #endregion

        #region 功能面板快捷键

        /// <summary>
        /// 获取或设置打开资源管理器面板快捷键。
        /// </summary>
        public required string OpenExplorerShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开搜索面板快捷键。
        /// </summary>
        public required string OpenSearchShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开源代码管理面板快捷键。
        /// </summary>
        public required string OpenSourceControlShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开运行/调试面板快捷键。
        /// </summary>
        public required string OpenRunDebugShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开用户偏好设置快捷键。
        /// </summary>
        public required string OpenUserPreferencesShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开性能监视器快捷键。
        /// </summary>
        public required string OpenUserPreformanceShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开主题设置快捷键。
        /// </summary>
        public required string OpenThemeShortcut { get; set; }

        /// <summary>
        /// 获取或设置打开语言设置快捷键。
        /// </summary>
        public required string OpenLanguageShortcut { get; set; }

        #endregion
    }
}

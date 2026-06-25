using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace OpticalChainSimulator.Views
{
    /// <summary>
    /// 项目信息展示用户控件
    /// 用于展示和管理光学链模拟器项目的核心信息（如项目名称、创建时间、描述、配置等）
    /// </summary>
    public partial class ProjectInfoControl : UserControl
    {
        /// <summary>
        /// 项目信息控件构造函数
        /// 控件实例化时的核心入口，调用初始化方法完成XAML资源加载
        /// </summary>
        public ProjectInfoControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件的XAML资源
        /// 封装Avalonia框架的XAML加载方法，完成控件界面元素的初始化
        /// 是Avalonia用户控件初始化的标准实现方式
        /// </summary>
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
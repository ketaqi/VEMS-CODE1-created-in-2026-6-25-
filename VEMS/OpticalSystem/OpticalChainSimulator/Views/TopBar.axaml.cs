using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dock.Model.Core;
using Dock.Model.Mvvm.Controls;
using OpticalChainSimulator.Factories;

namespace OpticalChainSimulator.Views
{
    /// <summary>
    /// 应用顶部导航栏用户控件
    /// 作为应用顶部操作栏的容器，预留布局切换、主题切换、布局保存/加载、关于/退出等功能入口（当前仅保留基础初始化结构）
    /// </summary>
    public partial class TopBar : UserControl
    {
        /// <summary>
        /// 顶部导航栏控件构造函数
        /// 调用初始化方法完成控件的基础创建
        /// </summary>
        public TopBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化控件的XAML资源
        /// 封装AvaloniaXamlLoader.Load方法，完成控件界面元素的加载和初始化
        /// 是Avalonia控件初始化的标准实现方式
        /// </summary>
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
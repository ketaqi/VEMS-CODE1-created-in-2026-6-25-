using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpticalChainSimulator.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;

namespace OpticalChainSimulator.Views
{
    /// <summary>
    /// 探测器结果展示窗口
    /// 核心功能：绘制模拟的探测器强度分布热力图（含色条），支持复制探测器结果文本到剪贴板、关闭窗口
    /// </summary>
    public partial class DetectorResultsWindow : Window
    {
        /// <summary>
        /// ScottPlot的Avalonia绘图控件实例，用于绘制探测器强度热力图
        /// </summary>
        private AvaPlot? _plot;

        /// <summary>
        /// 窗口构造函数
        /// 初始化控件、查找绘图控件、订阅数据上下文变化/窗口打开事件以刷新绘图
        /// </summary>
        public DetectorResultsWindow()
        {
            // 初始化窗口XAML资源
            InitializeComponent();

            // 查找XAML中命名为DetectorPlot的AvaPlot控件
            _plot = this.FindControl<AvaPlot>("DetectorPlot");

            // 订阅数据上下文变化事件，触发绘图刷新
            this.DataContextChanged += (_, __) => RefreshPlot();
            // 订阅窗口打开事件，触发绘图刷新（确保窗口加载后立即绘制）
            this.Opened += (_, __) => RefreshPlot();
        }

        /// <summary>
        /// 初始化窗口XAML资源
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// 核心绘图方法：生成模拟的探测器强度数据，绘制热力图（含主图+间隔+色条）
        /// </summary>
        private void RefreshPlot()
        {
            // 绘图控件未初始化时直接返回，避免空引用
            if (_plot == null)
                return;

            // 清空原有绘图内容，准备绘制新图
            _plot.Plot.Clear();

            // ===============================
            // 1. 生成主热力图的模拟数据（高斯分布+随机噪声）
            // ===============================
            // 热力图的像素分辨率（X/Y方向点数）
            int nx = 180;
            int ny = 140;
            // 存储主热力图数据的二维数组（行=Y，列=X）
            double[,] main = new double[ny, nx];

            // 高斯分布参数：控制峰值位置和扩散程度
            double sigmaX = 0.9;  // X方向标准差
            double sigmaY = 0.6;  // Y方向标准差
            double x0 = 0.3;      // X方向峰值偏移
            double y0 = -0.2;     // Y方向峰值偏移

            // 遍历每个像素点，计算高斯分布值并添加随机噪声
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = 0; ix < nx; ix++)
                {
                    // 将像素索引转换为物理坐标（X范围：-3~3 mm，Y范围：-2~2 mm）
                    double x = (ix / (double)(nx - 1) - 0.5) * 6.0;
                    double y = (iy / (double)(ny - 1) - 0.5) * 4.0;

                    // 计算高斯分布的归一化坐标
                    double gx = (x - x0) / sigmaX;
                    double gy = (y - y0) / sigmaY;

                    // 高斯分布公式 + 小幅度随机噪声（模拟测量误差）
                    double v = Math.Exp(-(gx * gx + gy * gy));
                    v += 0.02 * (Random.Shared.NextDouble() - 0.5);

                    // 确保数值非负（强度不能为负），存入主数据数组
                    main[iy, ix] = Math.Max(0, v);
                }
            }

            // ===============================
            // 2. 组合绘图数据：主热力图 + NaN间隔 + 色条
            // ===============================
            int barWidth = 10;   // 右侧色条的宽度（像素数）
            int gapWidth = 12;   // 主图和色条之间的间隔宽度（像素数）
            // 组合后总宽度 = 主图宽度 + 间隔宽度 + 色条宽度
            int totalX = nx + gapWidth + barWidth;
            double[,] combined = new double[ny, totalX];

            // 2.1 填充主热力图数据到组合数组的左侧
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = 0; ix < nx; ix++)
                    combined[iy, ix] = main[iy, ix];
            }

            // 2.2 填充间隔区域为NaN（🔥 关键：NaN区域不会被渲染，形成空白间隔）
            for (int iy = 0; iy < ny; iy++)
            {
                for (int ix = nx; ix < nx + gapWidth; ix++)
                    combined[iy, ix] = double.NaN;
            }

            // 2.3 填充右侧色条数据（从上到下数值从1到0渐变）
            for (int iy = 0; iy < ny; iy++)
            {
                double v = 1.0 - iy / (double)(ny - 1); // 色条数值渐变（1→0）
                for (int ix = 0; ix < barWidth; ix++)
                    combined[iy, ix + nx + gapWidth] = v;
            }

            // ===============================
            // 3. 绘制组合热力图并配置样式
            // ===============================
            // 添加热力图到绘图控件，使用组合数据数组
            var hm = _plot.Plot.Add.Heatmap(combined);
            // 设置热力图的配色方案（Viridis：蓝→绿→黄→红，适合科学可视化）
            hm.Colormap = new ScottPlot.Colormaps.Viridis();

            // ===============================
            // 4. 设置图表标题和坐标轴标签
            // ===============================
            _plot.Plot.Title("Detector Intensity Distribution (Simulated)"); // 图表标题
            _plot.Plot.XLabel("X (mm)");                                     // X轴标签（毫米）
            _plot.Plot.YLabel("Y (mm)");                                     // Y轴标签（毫米）

            // 隐藏网格线（热力图无需网格，更清晰）
            _plot.Plot.Grid.IsVisible = false;
            // 刷新绘图控件，显示最新绘制内容
            _plot.Refresh();
        }

        /// <summary>
        /// 关闭按钮点击事件处理方法
        /// 关闭当前探测器结果窗口
        /// </summary>
        /// <param name="sender">事件发送者（关闭按钮）</param>
        /// <param name="e">路由事件参数</param>
        private void OnCloseClicked(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 复制按钮点击事件处理方法
        /// 将探测器结果文本复制到系统剪贴板，兼容不同的剪贴板获取方式
        /// </summary>
        /// <param name="sender">事件发送者（复制按钮）</param>
        /// <param name="e">路由事件参数</param>
        private async void OnCopyClicked(object? sender, RoutedEventArgs e)
        {
            // 数据上下文为探测器元件ViewModel时，获取结果文本
            if (DataContext is OpticalElementViewModel vm)
            {
                // 结果文本默认值：模拟数据提示
                var text = vm.DetectorResultText ?? "（模拟探测器结果，用于展示）";

                try
                {
                    // 方式1：尝试从窗口直接获取剪贴板
                    var clipboard = this.Clipboard;
                    if (clipboard != null)
                    {
                        await clipboard.SetTextAsync(text);
                        return;
                    }

                    // 方式2：兜底方案，从顶层控件获取剪贴板（兼容不同Avalonia版本）
                    var tl = TopLevel.GetTopLevel(this);
                    if (tl?.Clipboard != null)
                    {
                        await tl.Clipboard.SetTextAsync(text);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // 捕获剪贴板操作异常，输出调试日志（不影响程序运行）
                    Debug.WriteLine("Copy to clipboard failed: " + ex);
                }
            }
        }
    }
}
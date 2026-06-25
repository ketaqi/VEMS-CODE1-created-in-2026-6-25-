using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using RoslynPad.Build;
using RoslynPad.UI;
using RoslynPad.Utilities;

namespace RoslynPad.ViewModels
{
    /// <summary>
    /// 全局输出结果视图模型，管理应用程序级别的输出显示。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此视图模型实现 <see cref="IResultsViewModel"/> 接口，
    /// 提供统一的输出结果管理功能，包括：
    /// <list type="bullet">
    ///   <item><description>添加和显示编译错误、还原结果等</description></item>
    ///   <item><description>复制所有输出到剪贴板</description></item>
    ///   <item><description>清空输出</description></item>
    ///   <item><description>自定义字体和字号</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public class GlobalResultsViewModel : NotificationObject, IResultsViewModel
    {
        /// <summary>
        /// 输出结果集合。
        /// </summary>
        private readonly ObservableCollection<IResultObject> _results;

        /// <summary>
        /// 命令提供程序。
        /// </summary>
        private readonly ICommandProvider _commands;

        /// <summary>
        /// 初始化 <see cref="GlobalResultsViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="commands">命令提供程序。</param>
        public GlobalResultsViewModel(ICommandProvider commands)
        {
            _commands = commands;
            _results = new ObservableCollection<IResultObject>();

            CopyAllResultsCommand = commands.Create(CopyAllOutputResults);
            ClearResultsCommand = commands.Create(ClearOutputResults);

            EditorFontFamily1 = "Consolas";
            FontSize = 12.0;
            _editorFontFamily1 = EditorFontFamily1;
        }

        /// <summary>
        /// 获取输出结果集合。
        /// </summary>
        /// <value>只读的结果对象集合。</value>
        public IEnumerable<IResultObject> Results => _results;

        /// <summary>
        /// 获取所有输出的文本表示。
        /// </summary>
        /// <value>格式化的输出文本，用于 TextBox 绑定。</value>
        /// <remarks>
        /// 不同类型的结果对象会以不同格式输出：
        /// <list type="bullet">
        ///   <item><description>���译错误：严重性、错误代码、消息、行号、列号</description></item>
        ///   <item><description>还原结果：严重性、消息</description></item>
        ///   <item><description>普通结果：标题、值、类型</description></item>
        /// </list>
        /// </remarks>
        public string ResultsText
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var item in Results)
                {
                    if (item is CompilationErrorResultObject err)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture,
                            $"{err.Severity}\t{err.ErrorCode}\t{err.Message}\t{err.LineNumber}\t{err.Column}");
                    }
                    else if (item is RestoreResultObject restore)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture,
                            $"{restore.Severity}\t{restore.Message}");
                    }
                    else if (item is ResultObject res)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture,
                            $"{res.Header}\t{res.Value}\t{res.Type}");
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 添加输出结果项。
        /// </summary>
        /// <param name="o">要添加的结果对象；若为 <see langword="null"/> 则忽略。</param>
        public void AddResult(IResultObject o)
        {
            if (o == null)
            {
                return;
            }

            _results.Add(o);
            OnPropertyChanged(nameof(ResultsText));
        }

        /// <summary>
        /// 清空所有输出结果。
        /// </summary>
        public void ClearOutputResults()
        {
            _results.Clear();
            OnPropertyChanged(nameof(ResultsText));
        }

        /// <summary>
        /// 复制所有输出到剪贴板。
        /// </summary>
        private void CopyAllOutputResults()
        {
            var builder = new StringBuilder();
            lock (_results)
            {
                foreach (var result in _results)
                {
                    result.WriteTo(builder);
                    builder.AppendLine();
                }
            }

            var allText = builder.ToString();
            RequestCopyText?.Invoke(allText);
            CopyToClipboard(allText);
        }

        /// <summary>
        /// 将文本复制到系统剪贴板。
        /// </summary>
        /// <param name="text">要复制的文本。</param>
        private async void CopyToClipboard(string text)
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.Windows.FirstOrDefault(w => w.IsActive)
                : null;

            if (window != null)
            {
                var clipboard = window.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text).ConfigureAwait(true);
                    Console.WriteLine("[CopyError] 已复制到剪贴板:  " + text);

                    var clipboardText = await clipboard.GetTextAsync().ConfigureAwait(true);
                    Console.WriteLine("[CopyError] 剪贴板内容: " + clipboardText);
                }
                else
                {
                    Console.WriteLine("[CopyError] 未能获取剪贴板对象（Clipboard 为 null）");
                }
            }
            else
            {
                Console.WriteLine("[CopyError] 未找��活动窗口，无法访问剪贴板");
            }
        }

        /// <summary>
        /// 请求复制文本事件（供界面层实现剪贴板功能）。
        /// </summary>
        public event Action<string>? RequestCopyText;

        /// <summary>
        /// 获取复制全部输出命令。
        /// </summary>
        public IDelegateCommand CopyAllResultsCommand { get; }

        /// <summary>
        /// 获取清空输出命令。
        /// </summary>
        public IDelegateCommand ClearResultsCommand { get; }

        #region 字体设置

        private string _editorFontFamily1;

        /// <summary>
        /// 获取或设置输出区域字体系列。
        /// </summary>
        /// <value>字体系列名称，默认为 "Consolas"。</value>
        public string EditorFontFamily1
        {
            get => _editorFontFamily1;
            set
            {
                if (_editorFontFamily1 != value)
                {
                    _editorFontFamily1 = value;
                    OnPropertyChanged(nameof(EditorFontFamily1));
                }
            }
        }

        private double _fontSize = 50;

        /// <summary>
        /// 获取或设置输出区域字体大小。
        /// </summary>
        /// <value>字体大小。</value>
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        /// <summary>
        /// 处理编辑器字体变更通知。
        /// </summary>
        /// <param name="newFont1">新的字体系列名称。</param>
        public void OnEditorFontFamilyChanged1(string newFont1)
        {
            EditorFontFamily1 = newFont1;
            Console.WriteLine($"[OpenDocumentViewModel] OutputFontFamily 已变更:  {EditorFontFamily1}");
        }

        #endregion
    }
}

// 全局通用命名空间导入
// 核心输入/命令
global using System.Windows.Input;
// Avalonia基础控件/布局/样式
global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Controls.Presenters;
global using Avalonia.Data.Converters;
global using Avalonia.Input;
global using Avalonia.Interactivity;
global using Avalonia.Layout;
global using Avalonia.Media;
global using Avalonia.Styling;
global using Avalonia.Threading;
// AvaloniaEdit编辑器核心
global using AvaloniaEdit;
global using AvaloniaEdit.CodeCompletion;
global using AvaloniaEdit.Folding;
global using AvaloniaEdit.Document;
global using AvaloniaEdit.Editing;
global using AvaloniaEdit.Highlighting;
global using AvaloniaEdit.Rendering;
global using AvaloniaEdit.Snippets;
// 兼容/别名定义（统一不同框架的命名差异）
global using ModifierKeys = Avalonia.Input.KeyModifiers;
global using MouseEventArgs = Avalonia.Input.PointerEventArgs;
global using TextCompositionEventArgs = Avalonia.Input.TextInputEventArgs;
global using CommonTextEventArgs = Avalonia.Input.TextInputEventArgs;
global using RoutingStrategy = Avalonia.Interactivity.RoutingStrategies;
global using FontStyles = Avalonia.Media.FontStyle;
global using FontWeights = Avalonia.Media.FontWeight;
global using CommonFontWeights = Avalonia.Media.FontWeight;
global using CommonBrush = Avalonia.Media.IBrush;
global using Brush = Avalonia.Media.IBrush;
global using ImageSource = Avalonia.Media.IImage;
global using CommonImage = Avalonia.Media.IImage;
// AvaloniaEdit命令/文档
global using AvalonEditCommands = AvaloniaEdit.AvaloniaEditCommands;
global using TextDocument = AvaloniaEdit.Document.TextDocument;
global using CommandBinding = AvaloniaEdit.RoutedCommandBinding;

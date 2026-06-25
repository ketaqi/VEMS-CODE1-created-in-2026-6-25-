using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPad.UI.Dialogs;

public interface ISaveFileDialog
{
    /// <summary>
    /// 文件类型过滤器（如 "C# Files|*.cs;*.csx"）
    /// </summary>
    FileDialogFilter? Filter { get; set; }

    /// <summary>
    /// 多文件类型过滤器列表（如支持多种类型）
    /// </summary>
    IList<FileDialogFilter>? Filters { get; set; } // 新增

    /// <summary>
    /// 默认文件名（如 "Program.cs"）
    /// </summary>
    string? InitialFileName { get; set; }

    /// <summary>
    /// 默认打开目录（如 "C:\Users\xxx\Documents"）
    /// </summary>
    string? InitialDirectory { get; set; }

    /// <summary>
    /// 默认扩展名（如 "json"）
    /// </summary>
    string? DefaultExtension { get; set; }

    /// <summary>
    /// 用户选择的文件路径
    /// </summary>
    string? FilePath { get; }

    /// <summary>
    /// 对话框标题
    /// </summary>
    string? Title { get; set; }

    /// <summary>
    /// 显示保存对话框，返回用户选择的路径（或 null 表示取消）
    /// </summary>
    Task<string?> ShowAsync();
}



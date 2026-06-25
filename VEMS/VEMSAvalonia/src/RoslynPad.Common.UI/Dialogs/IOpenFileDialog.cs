namespace RoslynPad.UI;


public interface IOpenFileDialog
{
    /// <summary>
    /// 是否允许多选
    /// </summary>
    bool AllowMultiple { get; set; }

    /// <summary>
    /// 单一文件类型过滤器
    /// </summary>
    FileDialogFilter? Filter { get; set; }

    /// <summary>
    /// 多文件类型过滤器列表
    /// </summary>
    IList<FileDialogFilter>? Filters { get; set; } // 新增

    /// <summary>
    /// 默认打开目录
    /// </summary>
    string? InitialDirectory { get; set; }

    /// <summary>
    /// 默认文件名
    /// </summary>
    string? FileName { get; set; }

    /// <summary>
    /// 对话框标题
    /// </summary>
    string? Title { get; set; } // 可选

    /// <summary>
    /// 显示打开文件对话框，返回用户选择的文件路径数组（或 null 表示取消）
    /// </summary>
    Task<string[]?> ShowAsync();
}


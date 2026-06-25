namespace RoslynPad.Editor;

/// <summary>
/// 定义代码编辑器补全提供程序的接口
/// </summary>
public interface ICodeEditorCompletionProvider
{
    /// <summary>
    /// 获取指定位置的代码补全数据
    /// </summary>
    /// <param name="position">代码补全的位置偏移量</param>
    /// <param name="triggerChar">触发代码补全的字符（可为空）</param>
    /// <param name="useSignatureHelp">是否启用签名帮助</param>
    /// <returns>包含补全结果的异步任务</returns>
    Task<CompletionResult> GetCompletionData(int position, char? triggerChar, bool useSignatureHelp);
}

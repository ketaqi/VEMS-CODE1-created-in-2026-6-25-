using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoslynPad.Roslyn;

/// <summary>
/// 实现 INotifyPropertyChanged 接口的抽象基类，提供属性变更通知能力
/// </summary>
internal abstract class NotificationObject : INotifyPropertyChanged
{
    /// <summary>
    /// 属性变更时触发的事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 设置属性值并在值变更时触发属性变更通知
    /// </summary>
    /// <typeparam name="T">属性类型</typeparam>
    /// <param name="field">字段引用</param>
    /// <param name="value">新值</param>
    /// <param name="propertyName">属性名称（自动填充，无需手动传入）</param>
    /// <returns>值是否发生变更</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 触发属性变更通知
    /// </summary>
    /// <param name="propertyName">属性名称（自动填充，无需手动传入）</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

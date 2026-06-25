using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RoslynPad.Utilities;

/// <summary>
/// 通知对象基类：实现 <see cref="INotifyPropertyChanged"/> 接口，为 MVVM 模式提供属性变更通知基础设施。
/// </summary>
/// <remarks>
/// <para>
/// 此抽象类是所有需要属性变更通知的 ViewModel 和 Model 类的基类。
/// 它提供了简化的属性设置方法，自动处理值比较和事件触发。
/// </para>
/// <para>
/// 主要功能：
/// <list type="bullet">
///   <item><description>通过 <see cref="SetProperty{T}"/> 方法简化属性 setter 的实现</description></item>
///   <item><description>自动进行值相等性检查，仅在值变化时触发通知</description></item>
///   <item><description>使用 <see cref="CallerMemberNameAttribute"/> 自动获取属性名称</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class PersonViewModel : NotificationObject
/// {
///     private string _name = string.Empty;
///     
///     public string Name
///     {
///         get => _name;
///         set => SetProperty(ref _name, value);
///     }
///     
///     private int _age;
///     
///     public int Age
///     {
///         get => _age;
///         set
///         {
///             if (SetProperty(ref _age, value))
///             {
///                 // 值已更改，执行额外逻辑
///                 OnPropertyChanged(nameof(IsAdult));
///             }
///         }
///     }
///     
///     public bool IsAdult => Age >= 18;
/// }
/// </code>
/// </example>
internal abstract class NotificationObject : INotifyPropertyChanged
{
    /// <summary>
    /// 当属性值更改时发生。
    /// </summary>
    /// <remarks>
    /// UI 框架（如 Avalonia、WPF）会订阅此事件以自动更新绑定的控件。
    /// </remarks>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 设置属性值，如果值发生变化则触发 <see cref="PropertyChanged"/> 事件。
    /// </summary>
    /// <typeparam name="T">属性值的类型。</typeparam>
    /// <param name="field">存储属性值的后备字段（通过引用传递）。</param>
    /// <param name="value">要设置的新值。</param>
    /// <param name="propertyName">
    /// 属性名称。通常无需手动指定，编译器会通过 <see cref="CallerMemberNameAttribute"/> 自动提供。
    /// </param>
    /// <returns>
    /// 如果值发生变化并触发了通知，返回 <c>true</c>；
    /// 如果新值与旧值相等，返回 <c>false</c>。
    /// </returns>
    /// <remarks>
    /// <para>
    /// 此方法使用 <see cref="EqualityComparer{T}.Default"/> 进行值比较，
    /// 支持引用类型、值类型和实现了 <see cref="IEquatable{T}"/> 的类型。
    /// </para>
    /// <para>
    /// 返回值可用于在值变化后执行额外逻辑，如触发相关属性的通知。
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// private string _title = string.Empty;
    /// 
    /// public string Title
    /// {
    ///     get => _title;
    ///     set
    ///     {
    ///         if (SetProperty(ref _title, value))
    ///         {
    ///             // 标题变化后，窗口标题也需要更新
    ///             OnPropertyChanged(nameof(WindowTitle));
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
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
    /// 触发 <see cref="PropertyChanged"/> 事件。
    /// </summary>
    /// <param name="propertyName">
    /// 已更改的属性名称。通常无需手动指定，编译器会通过 <see cref="CallerMemberNameAttribute"/> 自动提供。
    /// 传递 <c>null</c> 或空字符串表示所有属性都已更改。
    /// </param>
    /// <remarks>
    /// <para>
    /// 此方法可被派生类重写以添加额外的通知逻辑。
    /// </para>
    /// <para>
    /// 在需要手动通知计算属性变化时，可直接调用此方法：
    /// <code>
    /// OnPropertyChanged(nameof(FullName)); // 当 FirstName 或 LastName 变化时
    /// </code>
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

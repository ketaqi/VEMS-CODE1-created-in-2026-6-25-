using System;

namespace RoslynPad.FeatureDemos.Views
{
    /// <summary>
    /// 将按钮行为声明在模型属性上的特性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此特性用于在属性网格中为特定属性显示一个按钮，点击按钮时可以：
    /// <list type="bullet">
    ///   <item><description>调用模型实例上的方法</description></item>
    ///   <item><description>执行模型实例上的 <see cref="System.Windows.Input.ICommand"/></description></item>
    ///   <item><description>调用后自动设置属性值并刷新显示</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 方法查找顺序：
    /// <list type="number">
    ///   <item><description>首先在属性所在的模型实例上查找</description></item>
    ///   <item><description>若找不到，尝试在注入的 MainWindow 上查找</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 基本用法：
    /// <code language="csharp">
    /// public class MySettings
    /// {
    ///     // 调用无参方法
    ///     [PropertyButton("重置", MethodName = "Reset")]
    ///     public object ResetButton { get; set; }
    ///     
    ///     // 调用带参数的方法
    ///     [PropertyButton("设置默认值", MethodName = "SetValue", MethodParameter = 100)]
    ///     public object DefaultButton { get; set; }
    ///     
    ///     // 执行命令
    ///     [PropertyButton("保存", CommandPropertyName = "SaveCommand")]
    ///     public object SaveButton { get; set; }
    ///     
    ///     // 调用后设置属性值
    ///     [PropertyButton("清除", MethodName = "Clear", SetValueAfterInvoke = true, SetValue = null)]
    ///     public object ClearButton { get; set; }
    ///     
    ///     public ICommand SaveCommand { get; }
    ///     
    ///     public void Reset() { /* ...  */ }
    ///     public void SetValue(int value) { /* ... */ }
    ///     public void Clear() { /* ... */ }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OnlyButtonCellEditFactorytest"/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyButtonAttribute : Attribute
    {
        /// <summary>
        /// 获取按钮显示的文本内容。
        /// </summary>
        /// <value>按钮上显示的文字。</value>
        public string Content { get; }

        /// <summary>
        /// 获取或设置要调用的方法名称。
        /// </summary>
        /// <value>
        /// 方法名称；方法可以是无参或单参数的实例方法。
        /// 若为 <see langword="null"/>，则不调用方法。
        /// </value>
        /// <remarks>
        /// 若同时设置了 <see cref="CommandPropertyName"/>，则优先执行命令。
        /// </remarks>
        public string? MethodName { get; init; }

        /// <summary>
        /// 获取或设置要执行的 <see cref="System.Windows.Input. ICommand"/> 属性名称。
        /// </summary>
        /// <value>
        /// 模型实例上返回 <see cref="System.Windows.Input.ICommand"/> 的属性名称。
        /// 若为 <see langword="null"/>，则不执行命令。
        /// </value>
        public string? CommandPropertyName { get; init; }

        /// <summary>
        /// 获取或设置传给方法或命令的参数。
        /// </summary>
        /// <value>
        /// 调用方法时的参数值，或传递给 <see cref="System.Windows.Input.ICommand. Execute(object?)"/> 的参数。
        /// 可为 <see langword="null"/>。
        /// </value>
        public object? MethodParameter { get; init; }

        /// <summary>
        /// 获取或设置调用后是否自动设置属性值。
        /// </summary>
        /// <value>
        /// <see langword="true"/> 表示调用方法或命令后，将 <see cref="SetValue"/> 写回属性并刷新属性网格；
        /// <see langword="false"/> 表示不设置。默认为 <see langword="false"/>。
        /// </value>
        public bool SetValueAfterInvoke { get; init; }

        /// <summary>
        /// 获取或设置调用后要写回属性的值。
        /// </summary>
        /// <value>
        /// 当 <see cref="SetValueAfterInvoke"/> 为 <see langword="true"/> 时，
        /// 此值将被写入属性。可为 <see langword="null"/>。
        /// </value>
        public object? SetValue { get; init; }

        /// <summary>
        /// 初始化 <see cref="PropertyButtonAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="content">按钮显示的文本内容。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="content"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        public PropertyButtonAttribute(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}

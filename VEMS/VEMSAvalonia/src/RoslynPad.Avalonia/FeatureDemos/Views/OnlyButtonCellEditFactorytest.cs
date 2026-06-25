using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.ViewModels;

namespace RoslynPad.FeatureDemos.Views
{
    /// <summary>
    /// 基于 <see cref="PropertyButtonAttribute"/> 特性驱动的按钮单元格编辑工厂。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此工厂为标记了 <see cref="PropertyButtonAttribute"/> 的属性创建按钮控件，
    /// 点击按钮时可以执行指定的方法或命令。
    /// </para>
    /// <para>
    /// 支持的功能：
    /// <list type="bullet">
    ///   <item><description>调用模型实例上的方法（通过 <see cref="PropertyButtonAttribute. MethodName"/>）</description></item>
    ///   <item><description>执行模型实例上的 <see cref="ICommand"/>（通过 <see cref="PropertyButtonAttribute.CommandPropertyName"/>）</description></item>
    ///   <item><description>传递参数给方法或命令</description></item>
    ///   <item><description>调用后自动设置属性值并刷新属性网格</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 兼容性：此工厂通过反射查找上下文中的实例对象，以兼容不同版本的 PropertyCellContext 实现。
    /// </para>
    /// </remarks>
    /// <example>
    /// 在模型类中使用：
    /// <code language="csharp">
    /// public class MyModel
    /// {
    ///     [PropertyButton("执行操作", MethodName = "DoSomething")]
    ///     public object ActionButton { get; set; }
    ///     
    ///     public void DoSomething()
    ///     {
    ///         Console.WriteLine("操作已执行");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="PropertyButtonAttribute"/>
    internal class OnlyButtonCellEditFactorytest : AbstractCellEditFactory
    {
        /// <summary>
        /// 获取或设置主视图模型引用。
        /// </summary>
        /// <value>
        /// 主窗口的视图模型，用于在模型实例上找不到方法时作为备选调用目标。
        /// </value>
        /// <remarks>
        /// 此属性通常在控件附着到视觉树时由 <see cref="TestExtendPropertyGrid"/> 注入。
        /// </remarks>
        public MainViewModel? MainWindow { get; set; }

        /// <summary>
        /// 确定此工厂是否接受指定的访问令牌。
        /// </summary>
        /// <param name="accessToken">访问令牌对象。</param>
        /// <returns>若访问令牌是 <see cref="TestExtendPropertyGrid"/> 实例则返回 <see langword="true"/>。</returns>
        public override bool Accept(object accessToken) => accessToken is TestExtendPropertyGrid;

        /// <summary>
        /// 为标记了 <see cref="PropertyButtonAttribute"/> 的属性创建按钮控件。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>
        /// 若属性标记了 <see cref="PropertyButtonAttribute"/>，返回配置好的 <see cref="Button"/> 控件；
        /// 否则返回 <see langword="null"/>。
        /// </returns>
        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            if (context == null)
            {
                return null;
            }

            // 通过反射获取模型实例
            var instance = GetInstanceFromContext(context);
            if (instance == null)
            {
                Console.WriteLine("[OnlyButtonCellEditFactory] 警告：无法从 PropertyCellContext 解析模型实例。");
                return null;
            }

            var propertyName = context.Property?.Name;
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            // 获取属性信息和特性
            var propertyInfo = instance.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (propertyInfo == null)
            {
                return null;
            }

            var attribute = propertyInfo.GetCustomAttribute<PropertyButtonAttribute>(inherit: true);
            if (attribute == null)
            {
                return null;
            }

            // 创建按钮控件
            var button = new Button
            {
                Content = attribute.Content,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };

            // 配置点击事件处理
            button.Click += (_, _) => HandleButtonClick(context, instance, attribute);

            return button;
        }

        /// <summary>
        /// 处理属性值变更。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>始终返回 <see langword="true"/>。</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context) => true;

        #region 私有辅助方法

        /// <summary>
        /// 处理按钮点击事件。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <param name="instance">模型实例。</param>
        /// <param name="attribute">按钮特性配置。</param>
        private void HandleButtonClick(PropertyCellContext context, object instance, PropertyButtonAttribute attribute)
        {
            try
            {
                // 优先执行命令
                if (!string.IsNullOrEmpty(attribute.CommandPropertyName))
                {
                    ExecuteCommand(instance, attribute);
                }
                // 其次调用方法
                else if (!string.IsNullOrEmpty(attribute.MethodName))
                {
                    bool invoked = TryInvokeMethod(instance, attribute.MethodName, attribute.MethodParameter);

                    // 若在实例上找不到方法，尝试在 MainWindow 上调用
                    if (!invoked && MainWindow != null)
                    {
                        TryInvokeMethod(MainWindow, attribute.MethodName, attribute.MethodParameter);
                    }
                }

                // 调用后设置属性值
                if (attribute.SetValueAfterInvoke)
                {
                    context.Factory?.SetPropertyValue(context, attribute.SetValue);
                    context.Factory?.HandlePropertyChanged(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OnlyButtonCellEditFactory] 按钮处理异常：{ex}");
            }
        }

        /// <summary>
        /// 执行模型实例上的命令。
        /// </summary>
        /// <param name="instance">模型实例。</param>
        /// <param name="attribute">按钮特性配置。</param>
        private static void ExecuteCommand(object instance, PropertyButtonAttribute attribute)
        {
            var commandProperty = instance.GetType().GetProperty(
                attribute.CommandPropertyName!,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (commandProperty != null)
            {
                var commandObject = commandProperty.GetValue(instance);
                if (commandObject is ICommand command && command.CanExecute(attribute.MethodParameter))
                {
                    command.Execute(attribute.MethodParameter);
                }
            }
        }

        /// <summary>
        /// 尝试在目标对象上调用指定方法。
        /// </summary>
        /// <param name="target">目标对象。</param>
        /// <param name="methodName">方法名称。</param>
        /// <param name="parameter">方法参数；可为 <see langword="null"/>。</param>
        /// <returns>若成功调用方法返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        private static bool TryInvokeMethod(object target, string methodName, object? parameter)
        {
            if (target == null)
            {
                return false;
            }

            var targetType = target.GetType();

            // 首先尝试无参方法
            var parameterlessMethod = targetType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                Type.DefaultBinder,
                Type.EmptyTypes,
                null);

            if (parameterlessMethod != null)
            {
                parameterlessMethod.Invoke(target, null);
                return true;
            }

            // 尝试单参数方法
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.Name != methodName)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    continue;
                }

                // 处理值类型参数为 null 的情况
                if (parameter == null && parameters[0].ParameterType.IsValueType)
                {
                    continue;
                }

                object? argument = parameter;

                // 尝试类型转换
                try
                {
                    if (parameter != null && !parameters[0].ParameterType.IsAssignableFrom(parameter.GetType()))
                    {
                        argument = Convert.ChangeType(parameter, parameters[0].ParameterType, CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                    continue;
                }

                method.Invoke(target, new[] { argument });
                return true;
            }

            return false;
        }

        /// <summary>
        /// 通过反射从 PropertyCellContext 中获取模型实例。
        /// </summary>
        /// <param name="context">属性单元格上下文。</param>
        /// <returns>模型实例；若无法获取则返回 <see langword="null"/>。</returns>
        /// <remarks>
        /// <para>
        /// 此方法在 PropertyCellContext 上查找常见的属性或字段名，
        /// 以增强对不同库版本的兼容性。
        /// </para>
        /// <para>
        /// 查找的候选名称包括：Instance、Value、Source、Target、Owner、Model、
        /// RootObject、Item、SelectedObject、Object。
        /// </para>
        /// </remarks>
        private static object? GetInstanceFromContext(PropertyCellContext context)
        {
            if (context == null)
            {
                return null;
            }

            var contextType = context.GetType();

            // 候选属性/字段名列表
            string[] candidates =
            {
                "Instance", "Value", "Source", "Target", "Owner",
                "Model", "RootObject", "Item", "SelectedObject", "Object"
            };

            foreach (var name in candidates)
            {
                // 尝试属性
                var property = contextType.GetProperty(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (property != null)
                {
                    try
                    {
                        var value = property.GetValue(context);
                        if (value != null)
                        {
                            return value;
                        }
                    }
                    catch
                    {
                        // 忽略异常，继续尝试下一个
                    }
                }

                // 尝试字段
                var field = contextType.GetField(
                    name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                {
                    try
                    {
                        var value = field.GetValue(context);
                        if (value != null)
                        {
                            return value;
                        }
                    }
                    catch
                    {
                        // 忽略异常，继续尝试下一个
                    }
                }
            }

            return null;
        }

        #endregion
    }
}

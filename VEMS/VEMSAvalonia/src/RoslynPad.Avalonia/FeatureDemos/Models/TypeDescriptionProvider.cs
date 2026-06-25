using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyModels.Extensions;

namespace RoslynPad.FeatureDemos.Models
{
    /// <summary>
    /// 用于测试动态属性功能的自定义对象。
    /// </summary>
    /// <remarks>
    /// 此类演示了如何使用 <see cref="BrowsableAttribute"/> 控制属性在属性网格中的可见性。
    /// </remarks>
    public class TestCustomObject
    {
        /// <summary>
        /// 获取或设置第一个字符串属性。
        /// </summary>
        /// <value>第一个字符串值，默认为空字符串。</value>
        public string First { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置第二个字符串属性。
        /// </summary>
        /// <value>第二个字符串值，默认为空字符串。</value>
        public string Second { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置字符串数组。
        /// </summary>
        /// <value>包含示例字符串的数组，默认值为 <c>["ABC", "DEF", "HJK"]</c>。</value>
        /// <remarks>
        /// 此属性标记为 <see cref="BrowsableAttribute"/>(<see langword="false"/>)，
        /// 因此不会在属性网格中显示。
        /// </remarks>
        [Browsable(false)]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly. Global
        public string[] StringArray { get; set; } = new[] { "ABC", "DEF", "HJK" };
    }

    #region 辅助类

    /// <summary>
    /// 动态属性管理器，用于在运行时向类型添加动态属性描述符。
    /// </summary>
    /// <typeparam name="TTarget">目标类型。</typeparam>
    /// <remarks>
    /// <para>
    /// 此类通过 <see cref="TypeDescriptor"/> 机制向指定类型或实例注册自定义的
    /// <see cref="TypeDescriptionProvider"/>，从而实现动态添加属性的功能。
    /// </para>
    /// <para>
    /// 使用完毕后应调用 <see cref="Dispose"/> 方法移除注册的提供程序。
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// using var manager = new DynamicPropertyManager&lt;MyClass&gt;();
    /// var property = DynamicPropertyFactory.CreateProperty&lt;MyClass, string&gt;(
    ///     "DynamicName",
    ///     obj => obj?. Name,
    ///     (obj, value) => { if (obj != null) obj.Name = value; },
    ///     null);
    /// manager.Properties.Add(property);
    /// </code>
    /// </example>
    public class DynamicPropertyManager<TTarget> : IDisposable
    {
        /// <summary>
        /// 动态类型描述提供程序。
        /// </summary>
        private readonly DynamicTypeDescriptionProvider _provider;

        /// <summary>
        /// 目标实例（若为基于实例的管理器）。
        /// </summary>
        private readonly TTarget? _target;

        /// <summary>
        /// 初始化 <see cref="DynamicPropertyManager{TTarget}"/> 类的新实例（基于类型）。
        /// </summary>
        /// <remarks>
        /// 使用此构造函数时，动态属性将应用于 <typeparamref name="TTarget"/> 类型的所有实例。
        /// </remarks>
        public DynamicPropertyManager()
        {
            var type = typeof(TTarget);

            _provider = new DynamicTypeDescriptionProvider(type);
            TypeDescriptor.AddProvider(_provider, type);
        }

        /// <summary>
        /// 初始化 <see cref="DynamicPropertyManager{TTarget}"/> 类的新实例（基于实例）。
        /// </summary>
        /// <param name="target">要添加动态属性的目标实例。</param>
        /// <remarks>
        /// 使用此构造函数时，动态属性仅应用于指定的 <paramref name="target"/> 实例。
        /// </remarks>
        public DynamicPropertyManager(TTarget target)
        {
            _target = target;

            _provider = new DynamicTypeDescriptionProvider(typeof(TTarget));
            TypeDescriptor.AddProvider(_provider, target!);
        }

        /// <summary>
        /// 获取动态属性描述符集合。
        /// </summary>
        /// <value>可添加或移除动态属性的列表。</value>
        public IList<PropertyDescriptor> Properties => _provider.Properties;

        /// <summary>
        /// 释放资源并移除已注册的类型描述提供程序。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放托管资源。
        /// </summary>
        /// <param name="disposing">
        /// 若为 <see langword="true"/>，则释放托管资源；
        /// 若为 <see langword="false"/>，则仅释放非托管资源。
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                TypeDescriptor.RemoveProvider(_provider, _target ?? (object)typeof(TTarget));
            }
        }
    }

    /// <summary>
    /// 动态属性创建工厂。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此非泛型静态类用于创建 <see cref="DynamicPropertyDescriptor{TTarget, TProperty}"/> 实例，
    /// 避免在泛型类型上定义静态成员（CA1000 警告）。
    /// </para>
    /// </remarks>
    public static class DynamicPropertyFactory
    {
        /// <summary>
        /// 创建具有 getter 和 setter 的动态属性描述符。
        /// </summary>
        /// <typeparam name="TTargetType">目标对象的类型。</typeparam>
        /// <typeparam name="TPropertyType">属性值的类型。</typeparam>
        /// <param name="displayName">属性的显示名称。</param>
        /// <param name="getter">获取属性值的委托。</param>
        /// <param name="setter">设置属性值的委托；若为 <see langword="null"/>，则属性为只读。</param>
        /// <param name="attributes">应用于属性的特性数组；可为 <see langword="null"/>。</param>
        /// <returns>新创建的动态属性描述符。</returns>
        /// <example>
        /// <code language="csharp">
        /// var property = DynamicPropertyFactory.CreateProperty&lt;Person, string&gt;(
        ///     "FullName",
        ///     p => $"{p?. FirstName} {p?. LastName}",
        ///     null,  // 只读属性
        ///     new Attribute[] { new CategoryAttribute("个人信息") });
        /// </code>
        /// </example>
        public static DynamicPropertyDescriptor<TTargetType, TPropertyType> CreateProperty<TTargetType, TPropertyType>(
            string displayName,
            Func<TTargetType?, TPropertyType?> getter,
            Action<TTargetType?, TPropertyType?>? setter,
            Attribute[]? attributes) =>
            new(displayName, getter, setter, attributes ?? Array.Empty<Attribute>());

        /// <summary>
        /// 创建只读的动态属性描述符。
        /// </summary>
        /// <typeparam name="TTargetType">目标对象的类型。</typeparam>
        /// <typeparam name="TPropertyType">属性值的类型。</typeparam>
        /// <param name="displayName">属性的显示名称。</param>
        /// <param name="getHandler">获取属性值的委托。</param>
        /// <param name="attributes">应用于属性的特性数组；可为 <see langword="null"/>。</param>
        /// <returns>新创建的只读动态属性描述符。</returns>
        public static DynamicPropertyDescriptor<TTargetType, TPropertyType> CreateProperty<TTargetType, TPropertyType>(
            string displayName,
            Func<TTargetType?, TPropertyType?> getHandler,
            Attribute[]? attributes) =>
            CreateProperty<TTargetType, TPropertyType>(displayName, getHandler, null, attributes);
    }

    /// <summary>
    /// 动态类型描述提供程序，用于向类型添加自定义属性描述符。
    /// </summary>
    /// <remarks>
    /// 此类继承自 <see cref="TypeDescriptionProvider"/>，通过重写 <see cref="GetTypeDescriptor"/>
    /// 方法返回包含动态属性的自定义类型描述符。
    /// </remarks>
    public class DynamicTypeDescriptionProvider : TypeDescriptionProvider
    {
        /// <summary>
        /// 原始类型描述提供程序。
        /// </summary>
        private readonly TypeDescriptionProvider _provider;

        /// <summary>
        /// 动态属性描述符列表。
        /// </summary>
        private readonly List<PropertyDescriptor> _properties = new();

        /// <summary>
        /// 初始化 <see cref="DynamicTypeDescriptionProvider"/> 类的新实例。
        /// </summary>
        /// <param name="type">要扩展的目标类型。</param>
        public DynamicTypeDescriptionProvider(Type type) => _provider = TypeDescriptor.GetProvider(type);

        /// <summary>
        /// 获取动态属性描述符集合。
        /// </summary>
        /// <value>可添加自定义属性描述符的列表。</value>
        public IList<PropertyDescriptor> Properties => _properties;

        /// <summary>
        /// 获取指定类型和实例的类型描述符。
        /// </summary>
        /// <param name="objectType">对象的类型。</param>
        /// <param name="instance">对象实例；可为 <see langword="null"/>。</param>
        /// <returns>包含动态属性的自定义类型描述符。</returns>
        public override ICustomTypeDescriptor? GetTypeDescriptor(Type objectType, object? instance)
            => new DynamicCustomTypeDescriptor(this, _provider.GetTypeDescriptor(objectType, instance)!);

        /// <summary>
        /// 自定义类型描述符，合并原始属性和动态属性。
        /// </summary>
        private class DynamicCustomTypeDescriptor : CustomTypeDescriptor
        {
            /// <summary>
            /// 父级动态类型描述提供程序。
            /// </summary>
            private readonly DynamicTypeDescriptionProvider _provider;

            /// <summary>
            /// 初始化 <see cref="DynamicCustomTypeDescriptor"/> 类的新实例。
            /// </summary>
            /// <param name="provider">父级动态类型描述提供程序。</param>
            /// <param name="descriptor">原始类型描述符。</param>
            public DynamicCustomTypeDescriptor(DynamicTypeDescriptionProvider provider, ICustomTypeDescriptor descriptor)
                  : base(descriptor) => _provider = provider;

            /// <summary>
            /// 获取对象的所有属性。
            /// </summary>
            /// <returns>包含原始属性和动态属性的属性描述符集合。</returns>
            public override PropertyDescriptorCollection GetProperties() => GetProperties(null);

            /// <summary>
            /// 获取符合指定特性筛选条件的属性。
            /// </summary>
            /// <param name="attributes">用于筛选的特性数组；可为 <see langword="null"/>。</param>
            /// <returns>包含原始属性和动态属性的属性描述符集合。</returns>
            public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
            {
                var properties = new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());

                // 添加原始属性
                foreach (PropertyDescriptor property in base.GetProperties(attributes))
                {
                    properties.Add(property);
                }

                // 添加动态属性
                foreach (var property in _provider.Properties)
                {
                    properties.Add(property);
                }

                return properties;
            }
        }
    }

    /// <summary>
    /// 泛型动态属性描述符，支持通过委托定义属性的读写行为。
    /// </summary>
    /// <typeparam name="TTarget">属性所属对象的类型。</typeparam>
    /// <typeparam name="TProperty">属性值的类型。</typeparam>
    /// <remarks>
    /// <para>
    /// 此类允许在运行时动态创建属性，而无需在编译时定义。
    /// 属性的读取和写入行为通过委托指定。
    /// </para>
    /// <para>
    /// 若 <c>setter</c> 为 <see langword="null"/>，则属性为只读。
    /// </para>
    /// </remarks>
    public class DynamicPropertyDescriptor<TTarget, TProperty> : PropertyDescriptor
    {
        /// <summary>
        /// 获取属性值的委托。
        /// </summary>
        private readonly Func<TTarget?, TProperty?> _getter;

        /// <summary>
        /// 设置属性值的委托；若为 <see langword="null"/>，则属性只读。
        /// </summary>
        private readonly Action<TTarget?, TProperty?>? _setter;

        /// <summary>
        /// 属性名称。
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// 初始化 <see cref="DynamicPropertyDescriptor{TTarget, TProperty}"/> 类的新实例。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="getter">���取属性值的委托。</param>
        /// <param name="setter">设置属性值的委托；可为 <see langword="null"/> 表示只读。</param>
        /// <param name="attributes">应用于属性的特性数组；可为 <see langword="null"/>。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="getter"/> 为 <see langword="null"/> 时抛出。</exception>
        public DynamicPropertyDescriptor(
           string propertyName,
           Func<TTarget?, TProperty?> getter,
           Action<TTarget?, TProperty?>? setter,
           Attribute[]? attributes)
              : base(propertyName, attributes ?? Array.Empty<Attribute>())
        {
            _setter = setter;
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _propertyName = propertyName;
        }

        /// <summary>
        /// 确定指定对象是否等于当前动态属性描述符。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>若属性名称相同则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public override bool Equals(object? obj)
            => obj is DynamicPropertyDescriptor<TTarget, TProperty> other
            && other._propertyName.Equals(_propertyName, StringComparison.Ordinal);

        /// <summary>
        /// 获取当前动态属性描述符的哈希代码。
        /// </summary>
        /// <returns>基于属性名称的哈希代码。</returns>
        public override int GetHashCode() => _propertyName.GetHashCode();

        /// <summary>
        /// 确定是否可以重置指定组件上的属性值。
        /// </summary>
        /// <param name="component">要检查的组件。</param>
        /// <returns>始终返回 <see langword="true"/>。</returns>
        public override bool CanResetValue(object component) => true;

        /// <summary>
        /// 获取属性所属的组件类型。
        /// </summary>
        /// <value>返回 <typeparamref name="TTarget"/> 类型。</value>
        public override Type ComponentType => typeof(TTarget);

        /// <summary>
        /// 获取指定组件上的属性值。
        /// </summary>
        /// <param name="component">要获取属性值的组件。</param>
        /// <returns>属性值。</returns>
        public override object? GetValue(object? component) => _getter((TTarget?)component);

        /// <summary>
        /// 获取一个值，指示此属性是否为只读。
        /// </summary>
        /// <value>
        /// 当 <c>setter</c> 为 <see langword="null"/> 或应用了 
        /// <see cref="ReadOnlyAttribute"/>(<see langword="true"/>) 时返回 <see langword="true"/>；
        /// 否则返回 <see langword="false"/>。
        /// </value>
        public override bool IsReadOnly
        {
            get
            {
                if (_setter == null)
                {
                    return true;
                }

                var attr = this.GetCustomAttribute<ReadOnlyAttribute>();
                return attr?.IsReadOnly == true;
            }
        }

        /// <summary>
        /// 获取属性的类型。
        /// </summary>
        /// <value>返回 <typeparamref name="TProperty"/> 类型。</value>
        public override Type PropertyType => typeof(TProperty);

        /// <summary>
        /// 将指定组件上的属性值重置为默认值。
        /// </summary>
        /// <param name="component">要重置属性的组件。</param>
        /// <remarks>此实现不执行任何操作。</remarks>
        public override void ResetValue(object component)
        { }

        /// <summary>
        /// 设置指定组件上的属性值。
        /// </summary>
        /// <param name="component">要设置属性值的组件。</param>
        /// <param name="value">要设置的新值。</param>
        /// <remarks>若 <c>setter</c> 为 <see langword="null"/>，则此方法不执行任何操作。</remarks>
        public override void SetValue(object? component, object? value) => _setter?.Invoke((TTarget?)component, (TProperty?)value);

        /// <summary>
        /// 确定是否应序列化指定组件的属性值。
        /// </summary>
        /// <param name="component">要检查的组件。</param>
        /// <returns>始终返回 <see langword="true"/>。</returns>
        public override bool ShouldSerializeValue(object component) => true;
    }

    #endregion
}

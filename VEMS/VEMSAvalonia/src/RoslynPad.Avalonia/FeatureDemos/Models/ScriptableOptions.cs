using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyModels.ComponentModel;

namespace RoslynPad.FeatureDemos.Models
{
    /// <summary>
    /// Class ScriptableOptions.
    /// Implements the <see cref="ReactiveObject" />
    /// Implements the <see cref="ICustomTypeDescriptor" />
    /// </summary>
    /// <seealso cref="ReactiveObject" />
    /// <seealso cref="ICustomTypeDescriptor" />
    public class ScriptableOptions : ReactiveObject, ICustomTypeDescriptor
    {
        /// <summary>
        /// The properties
        /// </summary>
#pragma warning disable CA1051 // 不要声明可见实例字段
        public readonly List<ScriptableObject> Properties = [];
#pragma warning restore CA1051 // 不要声明可见实例字段

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddProperty(ScriptableObject property) => Properties.Add(property);

        #region Custom Type Descriptor Interfaces

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);

        /// <summary>
        /// Returns the class name of this instance of a component.
        /// </summary>
        /// <returns>The class name of the object, or <see langword="null" /> if the class does not have a name.</returns>
        public string? GetClassName() => TypeDescriptor.GetClassName(this, true);

        /// <summary>
        /// Returns the name of this instance of a component.
        /// </summary>
        /// <returns>The name of the object, or <see langword="null" /> if the object does not have a name.</returns>
        public string? GetComponentName() => TypeDescriptor.GetComponentName(this, true);

        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);

        public EventDescriptor? GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);


        public PropertyDescriptor? GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);

        public object? GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);

        public EventDescriptorCollection GetEvents(Attribute[]? attributes) => TypeDescriptor.GetEvents(this, attributes, true);

        public PropertyDescriptorCollection GetProperties() => GetProperties(null);

        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            var pds = new PropertyDescriptorCollection(null);

            foreach (var property in Properties)
            {
                var pd = new ScriptableObjectPropertyDescriptor(property);

                pds.Add(pd);
            }

            return pds;
        }

        public object GetPropertyOwner(PropertyDescriptor? pd) => this;
        #endregion
    }

    #region Property Descriptor

    /// <summary>
    /// Class ScriptableObjectPropertyDescriptor.
    /// Implements the <see cref="PropertyDescriptor" />
    /// </summary>
    /// <seealso cref="PropertyDescriptor" />
    internal class ScriptableObjectPropertyDescriptor : PropertyDescriptor
    {
        private readonly Type _type;
        private readonly string? _displayName;
        private readonly string? _description;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptableObjectPropertyDescriptor" /> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public ScriptableObjectPropertyDescriptor(ScriptableObject target)
            : base(target.Name!, target.GetAttributes())
        {
            _type = target.ValueType!;
            _displayName = target.DisplayName;
            _description = target.Description;
        }

        public override Type ComponentType => typeof(ScriptableOptions);

        public override bool IsReadOnly => Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute
        {
            IsReadOnly: true
        };

        // ReSharper disable once FunctionRecursiveOnAllPaths
        // ReSharper disable once ConvertToAutoProperty
        public override Type PropertyType => _type;

        public override string DisplayName => _displayName ?? string.Empty;

        public override string Description => _description ?? string.Empty;

        public override bool CanResetValue(object component) => false;

        public override object? GetValue(object? component)
        {
            if (component is ScriptableOptions options)
            {
                var obj = options.Properties.Find(x => x.Name == Name);

                return obj?.Value;
            }

            return null;
        }

        public override void ResetValue(object component)
        { }

        public override void SetValue(object? component, object? value)
        {
            if (component is ScriptableOptions options)
            {
                var obj = options.Properties.Find(x => x.Name == Name);

                if (obj != null)
                {
                    obj.Value = value;

                    options.RaisePropertyChanged(Name);
                }
            }
        }

        public override bool ShouldSerializeValue(object component) => throw new NotImplementedException();
    }

    #endregion

}

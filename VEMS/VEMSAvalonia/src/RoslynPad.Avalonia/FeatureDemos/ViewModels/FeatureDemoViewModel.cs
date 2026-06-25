using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Avalonia.PropertyGrid.Services;
using PropertyModels.ComponentModel;
using RoslynPad.FeatureDemos.Models;
using RoslynPad.ViewModels;
using AAA = RoslynPad.FeatureDemos.Models;

namespace RoslynPad.FeatureDemos.ViewModels
{
    /// <summary>
    /// 功能演示视图模型，用于展示属性网格的各种功能特性。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此视图模型包含多个演示对象，用于展示属性网格支持的不同功能：
    /// <list type="bullet">
    ///   <item><description>简单对象属性编辑（<see cref="SimpleObject1"/> - <see cref="SimpleObject4"/>）</description></item>
    ///   <item><description>命令历史记录（<see cref="CommandHistory"/>）</description></item>
    ///   <item><description>可脚本化的自定义选项（<see cref="CustomOptions"/>）</description></item>
    ///   <item><description>动态属性管理器的使用示例</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// 使用示例：
    /// <code language="csharp">
    /// var mainViewModel = new MainViewModel();
    /// var demoViewModel = new FeatureDemoViewModel(mainViewModel);
    /// 
    /// // 访问简单对象
    /// var simpleObj = demoViewModel.SimpleObject1;
    /// 
    /// // 访问自定义选项
    /// var options = demoViewModel. CustomOptions;
    /// </code>
    /// </example>
    public class FeatureDemoViewModel : MiniReactiveObject
    {
        /// <summary>
        /// 主视图模型的引用。
        /// </summary>
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// 获取第一个简单测试对象。
        /// </summary>
        /// <value>用于演示基本属性编辑的 <see cref="Models.SimpleObject1"/> 实例。</value>
        public SimpleObject1? SimpleObject1 { get; }

        /// <summary>
        /// 获取第二个简单测试对象。
        /// </summary>
        /// <value>用于演示基本属性编辑的 <see cref="Models. SimpleObject2"/> 实例。</value>
        public SimpleObject2? SimpleObject2 { get; }

        /// <summary>
        /// 获取第三个简单测试对象。
        /// </summary>
        /// <value>用于演示基本属性编辑的 <see cref="Models. SimpleObject3"/> 实例。</value>
        public SimpleObject3? SimpleObject3 { get; }

        /// <summary>
        /// 获取第四个简单测试对象。
        /// </summary>
        /// <value>用于演示基本属性编辑的 <see cref="Models. SimpleObject4"/> 实例。</value>
        public SimpleObject4? SimpleObject4 { get; }

        /// <summary>
        /// 获取命令历史记录视图模型。
        /// </summary>
        /// <value>用于跟踪和显示用户操作历史的 <see cref="CommandHistoryViewModel"/> 实例。</value>
        public CommandHistoryViewModel CommandHistory { get; } = new();

        /// <summary>
        /// 获取可脚本化的自定义选项集合。
        /// </summary>
        /// <value>
        /// 包含动态生成属性的 <see cref="ScriptableOptions"/> 实例，
        /// 用于演示运行时添加自定义属性的功能。
        /// </value>
        public ScriptableOptions CustomOptions { get; } = new();

        /// <summary>
        /// 初始化 <see cref="FeatureDemoViewModel"/> 类的新实例。
        /// </summary>
        /// <param name="mainViewModel">主视图模型实例。</param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="mainViewModel"/> 为 <see langword="null"/> 时抛出。
        /// </exception>
        /// <remarks>
        /// <para>
        /// 构造函数执行以下初始化操作：
        /// <list type="number">
        ///   <item><description>创建四个简单测试对象实例</description></item>
        ///   <item><description>生成自定义选项属性</description></item>
        ///   <item><description>配置动态属性管理器，为 <see cref="TestCustomObject"/> 添加动态属性</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public FeatureDemoViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

            // 初始化简单测试对象
            SimpleObject1 = new SimpleObject1(_mainViewModel, "SimpleTests1");
            SimpleObject2 = new SimpleObject2(_mainViewModel, "SimpleTests2");
            SimpleObject3 = new SimpleObject3(_mainViewModel, "SimpleTests2");
            SimpleObject4 = new SimpleObject4(_mainViewModel, "SimpleTests2");

            // 生成自定义选项
            GenOptions();

            // 配置动态属性管理器
            ConfigureDynamicProperties();
        }

        /// <summary>
        /// 配置动态属性管理器，为 <see cref="TestCustomObject"/> 添加动态属性。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 此方法演示了如何使用 <see cref="DynamicPropertyManager{TTarget}"/> 
        /// 在运行时为类型添加动态属性。
        /// </para>
        /// <para>
        /// 添加的动态属性包括：
        /// <list type="bullet">
        ///   <item><description>StringArray0 - 可读写的字符串数组第一个元素</description></item>
        ///   <item><description>StringArray1 - 可读写的字符串数组第二个元素</description></item>
        ///   <item><description>StringArray2 - 只读的字符串数组第三个元素</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        private void ConfigureDynamicProperties()
        {
            var propertyManager = new DynamicPropertyManager<TestCustomObject>();

            // 添加可读写的 StringArray[0] 属性
            propertyManager.Properties.Add(AAA.DynamicPropertyFactory.CreateProperty<TestCustomObject, string>(
                "StringArray0",
                x => x!.StringArray[0],
                (x, v) => x!.StringArray[0] = v!,
                []
            ));

            // 添加可读写的 StringArray[1] 属性
            propertyManager.Properties.Add(AAA.DynamicPropertyFactory.CreateProperty<TestCustomObject, string>(
                "StringArray1",
                x => x!.StringArray[1],
                (x, v) => x!.StringArray[1] = v!,
                []
            ));

            // 添加只读的 StringArray[2] 属性
            propertyManager.Properties.Add(AAA.DynamicPropertyFactory.CreateProperty<TestCustomObject, string>(
                "StringArray2",
                x => x!.StringArray[2],
                (x, v) => x!.StringArray[2] = v!,
                [new ReadOnlyAttribute(true)]
            ));
        }

        #region 生成自定义对象属性

        /// <summary>
        /// 生成自定义选项属性。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 此方法向 <see cref="CustomOptions"/> 添加多种类型的可脚本化属性，
        /// 用于演示属性网格对不同数据类型的支持。
        /// </para>
        /// <para>
        /// 添加的属性类型包括：
        /// <list type="bullet">
        ///   <item><description>整数值（IntValue）</description></item>
        ///   <item><description>布尔值（BooleanValue）</description></item>
        ///   <item><description>可空布尔值/三态（ThreeBooleanValue）</description></item>
        ///   <item><description>字符串值（StringValue）</description></item>
        ///   <item><description>枚举值（EnumValue、EnumValueR）</description></item>
        ///   <item><description>绑定列表（BindingList、BindingListNotEditable、BindingListReadOnly）</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        private void GenOptions()
        {
            // 添加整数类型属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "IntValue",
                DisplayName = "Int Value",
                Description = "Custom type = int",
                Value = 1024
            });

            // 添加布尔类型属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "BooleanValue",
                DisplayName = "Boolean Value",
                Description = "Custom type = boolean",
                Value = true
            });

            // 添加可空布尔类型（三态）属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "ThreeBooleanValue",
                DisplayName = "Three Boolean Value",
                Description = "Custom type = boolean?",
                Value = null,
                ValueType = typeof(bool?)
            });

            // 添加字符串类型属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "StringValue",
                DisplayName = "String Value",
                Description = "Custom type = string",
                Value = "bodong"
            });

            // 添加枚举类型属性（带自定义验证）
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "EnumValue",
                DisplayName = "Enum Value",
                Description = "Custom type = Enum",
                Value = Environment.OSVersion.Platform,
                ExtraAttributes = [new ValidatePlatformAttribute()]
            });

            // 添加只读枚举类型属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "EnumValueR",
                DisplayName = "Enum Value(Readonly)",
                Description = "Custom type = Enum",
                Value = Environment.OSVersion.Platform,
                ExtraAttributes = [new ReadOnlyAttribute(true)]
            });

            // 添加可编辑的绑定列表属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "BindingList",
                DisplayName = "BindingList Value",
                Description = "Custom type = BindingList",
                Value = new BindingList<int> { 1024, 2048, 4096 },
                ExtraAttributes = [new CategoryAttribute("BindingList")]
            });

            // 添加不可编辑的绑定列表属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "BindingListNotEditable",
                DisplayName = "Not Editable List",
                Description = "Custom type = BindingList(Not Editable)",
                Value = new BindingList<int> { 1024, 2048, 4096 },
                ExtraAttributes = [new CategoryAttribute("BindingList"), new EditableAttribute(false)]
            });

            // 添加只读的绑定列表属性
            CustomOptions.AddProperty(new ScriptableObject
            {
                Name = "BindingListReadOnly",
                DisplayName = "ReadOnly List",
                Description = "Custom type = BindingList(Readonly)",
                Value = new BindingList<int> { 1024, 2048, 4096 },
                ExtraAttributes = [new CategoryAttribute("BindingList"), new ReadOnlyAttribute(true)]
            });
        }

        #endregion
    }
}

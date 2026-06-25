namespace RoslynPad.Roslyn.Completion;

/// <summary>
/// 表示代码补全项对应的图标类型枚举
/// </summary>
public enum Glyph
{
    /// <summary>无图标</summary>
    None,

    /// <summary>程序集图标</summary>
    Assembly,

    /// <summary>Basic 语言文件图标</summary>
    BasicFile,
    /// <summary>Basic 语言项目图标</summary>
    BasicProject,

    /// <summary>公共类图标</summary>
    ClassPublic,
    /// <summary>受保护类图标</summary>
    ClassProtected,
    /// <summary>私有类图标</summary>
    ClassPrivate,
    /// <summary>内部类图标</summary>
    ClassInternal,

    /// <summary>C# 语言文件图标</summary>
    CSharpFile,
    /// <summary>C# 语言项目图标</summary>
    CSharpProject,

    /// <summary>公共常量图标</summary>
    ConstantPublic,
    /// <summary>受保护常量图标</summary>
    ConstantProtected,
    /// <summary>私有常量图标</summary>
    ConstantPrivate,
    /// <summary>内部常量图标</summary>
    ConstantInternal,

    /// <summary>公共委托图标</summary>
    DelegatePublic,
    /// <summary>受保护委托图标</summary>
    DelegateProtected,
    /// <summary>私有委托图标</summary>
    DelegatePrivate,
    /// <summary>内部委托图标</summary>
    DelegateInternal,

    /// <summary>公共枚举图标</summary>
    EnumPublic,
    /// <summary>受保护枚举图标</summary>
    EnumProtected,
    /// <summary>私有枚举图标</summary>
    EnumPrivate,
    /// <summary>内部枚举图标</summary>
    EnumInternal,

    /// <summary>公共枚举成员图标</summary>
    EnumMemberPublic,
    /// <summary>受保护枚举成员图标</summary>
    EnumMemberProtected,
    /// <summary>私有枚举成员图标</summary>
    EnumMemberPrivate,
    /// <summary>内部枚举成员图标</summary>
    EnumMemberInternal,

    /// <summary>错误状态图标</summary>
    Error,
    /// <summary>状态信息图标</summary>
    StatusInformation,

    /// <summary>公共事件图标</summary>
    EventPublic,
    /// <summary>受保护事件图标</summary>
    EventProtected,
    /// <summary>私有事件图标</summary>
    EventPrivate,
    /// <summary>内部事件图标</summary>
    EventInternal,

    /// <summary>公共扩展方法图标</summary>
    ExtensionMethodPublic,
    /// <summary>受保护扩展方法图标</summary>
    ExtensionMethodProtected,
    /// <summary>私有扩展方法图标</summary>
    ExtensionMethodPrivate,
    /// <summary>内部扩展方法图标</summary>
    ExtensionMethodInternal,

    /// <summary>公共字段图标</summary>
    FieldPublic,
    /// <summary>受保护字段图标</summary>
    FieldProtected,
    /// <summary>私有字段图标</summary>
    FieldPrivate,
    /// <summary>内部字段图标</summary>
    FieldInternal,

    /// <summary>公共接口图标</summary>
    InterfacePublic,
    /// <summary>受保护接口图标</summary>
    InterfaceProtected,
    /// <summary>私有接口图标</summary>
    InterfacePrivate,
    /// <summary>内部接口图标</summary>
    InterfaceInternal,

    /// <summary>内置类型图标</summary>
    Intrinsic,

    /// <summary>关键字图标</summary>
    Keyword,

    /// <summary>标签图标</summary>
    Label,

    /// <summary>局部变量图标</summary>
    Local,

    /// <summary>命名空间图标</summary>
    Namespace,

    /// <summary>公共方法图标</summary>
    MethodPublic,
    /// <summary>受保护方法图标</summary>
    MethodProtected,
    /// <summary>私有方法图标</summary>
    MethodPrivate,
    /// <summary>内部方法图标</summary>
    MethodInternal,

    /// <summary>公共模块图标</summary>
    ModulePublic,
    /// <summary>受保护模块图标</summary>
    ModuleProtected,
    /// <summary>私有模块图标</summary>
    ModulePrivate,
    /// <summary>内部模块图标</summary>
    ModuleInternal,

    /// <summary>打开文件夹图标</summary>
    OpenFolder,

    /// <summary>运算符图标</summary>
    Operator,

    /// <summary>参数图标</summary>
    Parameter,

    /// <summary>公共属性图标</summary>
    PropertyPublic,
    /// <summary>受保护属性图标</summary>
    PropertyProtected,
    /// <summary>私有属性图标</summary>
    PropertyPrivate,
    /// <summary>内部属性图标</summary>
    PropertyInternal,

    /// <summary>范围变量图标</summary>
    RangeVariable,

    /// <summary>引用图标</summary>
    Reference,

    /// <summary>公共结构体图标</summary>
    StructurePublic,
    /// <summary>受保护结构体图标</summary>
    StructureProtected,
    /// <summary>私有结构体图标</summary>
    StructurePrivate,
    /// <summary>内部结构体图标</summary>
    StructureInternal,

    /// <summary>类型参数图标</summary>
    TypeParameter,

    /// <summary>代码片段图标</summary>
    Snippet,

    /// <summary>补全警告图标</summary>
    CompletionWarning,

    /// <summary>添加引用图标</summary>
    AddReference,
    /// <summary>NuGet 包图标</summary>
    NuGet,
    /// <summary>目标类型匹配图标</summary>
    TargetTypeMatch
}

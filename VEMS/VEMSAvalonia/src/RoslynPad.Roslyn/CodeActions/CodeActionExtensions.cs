using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Tags;
using Glyph = RoslynPad.Roslyn.Completion.Glyph;

namespace RoslynPad.Roslyn.CodeActions;

/// <summary>
/// 为CodeAction类型提供的扩展方法类，主要用于获取CodeAction的嵌套操作、图标样式等信息
/// </summary>
public static class CodeActionExtensions
{
    /// <summary>
    /// 检查指定的CodeAction是否包含嵌套的CodeAction集合
    /// </summary>
    /// <param name="codeAction">要检查的CodeAction实例，不能为null</param>
    /// <returns>如果包含非空的嵌套CodeAction集合则返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当codeAction参数为null时抛出</exception>
    public static bool HasCodeActions(this CodeAction codeAction)
    {
        ArgumentNullException.ThrowIfNull(codeAction);

        return !codeAction.NestedCodeActions.IsDefaultOrEmpty;
    }

    /// <summary>
    /// 获取指定CodeAction的嵌套CodeAction集合
    /// </summary>
    /// <param name="codeAction">要获取嵌套操作的CodeAction实例，不能为null</param>
    /// <returns>不可变的CodeAction数组，可能为空但不会为null</returns>
    /// <exception cref="ArgumentNullException">当codeAction参数为null时抛出</exception>
    public static ImmutableArray<CodeAction> GetCodeActions(this CodeAction codeAction)
    {
        ArgumentNullException.ThrowIfNull(codeAction);

        return codeAction.NestedCodeActions;
    }

    /// <summary>
    /// 根据CodeAction的标签集合获取对应的图标样式
    /// </summary>
    /// <param name="codeAction">要获取图标的CodeAction实例，不能为null</param>
    /// <returns>与CodeAction标签匹配的Glyph枚举值，无匹配时返回Glyph.None</returns>
    /// <exception cref="ArgumentNullException">当codeAction参数为null时抛出</exception>
    public static Glyph GetGlyph(this CodeAction codeAction)
    {
        ArgumentNullException.ThrowIfNull(codeAction);

        return GetGlyph(codeAction.Tags);
    }

    /// <summary>
    /// 根据标签集合获取对应的图标样式
    /// </summary>
    /// <param name="tags">代码元素的标签集合，包含元素类型、访问修饰符等信息</param>
    /// <returns>与标签匹配的Glyph枚举值，无匹配时返回Glyph.None</returns>
    public static Glyph GetGlyph(ImmutableArray<string> tags)
    {
        foreach (var tag in tags)
        {
            switch (tag)
            {
                case WellKnownTags.Assembly:
                    return Glyph.Assembly;

                case WellKnownTags.File:
                    return tags.Contains(LanguageNames.VisualBasic) ? Glyph.BasicFile : Glyph.CSharpFile;

                case WellKnownTags.Project:
                    return tags.Contains(LanguageNames.VisualBasic) ? Glyph.BasicProject : Glyph.CSharpProject;

                case WellKnownTags.Class:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.ClassProtected,
                        Accessibility.Private => Glyph.ClassPrivate,
                        Accessibility.Internal => Glyph.ClassInternal,
                        _ => Glyph.ClassPublic,
                    };
                case WellKnownTags.Constant:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.ConstantProtected,
                        Accessibility.Private => Glyph.ConstantPrivate,
                        Accessibility.Internal => Glyph.ConstantInternal,
                        _ => Glyph.ConstantPublic,
                    };
                case WellKnownTags.Delegate:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.DelegateProtected,
                        Accessibility.Private => Glyph.DelegatePrivate,
                        Accessibility.Internal => Glyph.DelegateInternal,
                        _ => Glyph.DelegatePublic,
                    };
                case WellKnownTags.Enum:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.EnumProtected,
                        Accessibility.Private => Glyph.EnumPrivate,
                        Accessibility.Internal => Glyph.EnumInternal,
                        _ => Glyph.EnumPublic,
                    };
                case WellKnownTags.EnumMember:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.EnumMemberProtected,
                        Accessibility.Private => Glyph.EnumMemberPrivate,
                        Accessibility.Internal => Glyph.EnumMemberInternal,
                        _ => Glyph.EnumMemberPublic,
                    };
                case WellKnownTags.Error:
                    return Glyph.Error;

                case WellKnownTags.Event:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.EventProtected,
                        Accessibility.Private => Glyph.EventPrivate,
                        Accessibility.Internal => Glyph.EventInternal,
                        _ => Glyph.EventPublic,
                    };
                case WellKnownTags.ExtensionMethod:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.ExtensionMethodProtected,
                        Accessibility.Private => Glyph.ExtensionMethodPrivate,
                        Accessibility.Internal => Glyph.ExtensionMethodInternal,
                        _ => Glyph.ExtensionMethodPublic,
                    };
                case WellKnownTags.Field:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.FieldProtected,
                        Accessibility.Private => Glyph.FieldPrivate,
                        Accessibility.Internal => Glyph.FieldInternal,
                        _ => Glyph.FieldPublic,
                    };
                case WellKnownTags.Interface:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.InterfaceProtected,
                        Accessibility.Private => Glyph.InterfacePrivate,
                        Accessibility.Internal => Glyph.InterfaceInternal,
                        _ => Glyph.InterfacePublic,
                    };
                case WellKnownTags.Intrinsic:
                    return Glyph.Intrinsic;

                case WellKnownTags.Keyword:
                    return Glyph.Keyword;

                case WellKnownTags.Label:
                    return Glyph.Label;

                case WellKnownTags.Local:
                    return Glyph.Local;

                case WellKnownTags.Namespace:
                    return Glyph.Namespace;

                case WellKnownTags.Method:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.MethodProtected,
                        Accessibility.Private => Glyph.MethodPrivate,
                        Accessibility.Internal => Glyph.MethodInternal,
                        _ => Glyph.MethodPublic,
                    };
                case WellKnownTags.Module:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.ModulePublic,
                        Accessibility.Private => Glyph.ModulePrivate,
                        Accessibility.Internal => Glyph.ModuleInternal,
                        _ => Glyph.ModulePublic,
                    };
                case WellKnownTags.Folder:
                    return Glyph.OpenFolder;

                case WellKnownTags.Operator:
                    return Glyph.Operator;

                case WellKnownTags.Parameter:
                    return Glyph.Parameter;

                case WellKnownTags.Property:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.PropertyProtected,
                        Accessibility.Private => Glyph.PropertyPrivate,
                        Accessibility.Internal => Glyph.PropertyInternal,
                        _ => Glyph.PropertyPublic,
                    };
                case WellKnownTags.RangeVariable:
                    return Glyph.RangeVariable;

                case WellKnownTags.Reference:
                    return Glyph.Reference;

                case WellKnownTags.NuGet:
                    return Glyph.NuGet;

                case WellKnownTags.Structure:
                    return GetAccessibility(tags) switch
                    {
                        Accessibility.Protected => Glyph.StructureProtected,
                        Accessibility.Private => Glyph.StructurePrivate,
                        Accessibility.Internal => Glyph.StructureInternal,
                        _ => Glyph.StructurePublic,
                    };
                case WellKnownTags.TypeParameter:
                    return Glyph.TypeParameter;

                case WellKnownTags.Snippet:
                    return Glyph.Snippet;

                case WellKnownTags.Warning:
                    return Glyph.CompletionWarning;

                case WellKnownTags.StatusInformation:
                    return Glyph.StatusInformation;
            }
        }

        return Glyph.None;
    }

    /// <summary>
    /// 从标签集合中解析代码元素的访问修饰符
    /// </summary>
    /// <param name="tags">包含访问修饰符标签的集合</param>
    /// <returns>匹配的Accessibility枚举值，无匹配时返回Accessibility.NotApplicable</returns>
    private static Accessibility GetAccessibility(ImmutableArray<string> tags)
    {
        if (tags.Contains(WellKnownTags.Public))
        {
            return Accessibility.Public;
        }
        if (tags.Contains(WellKnownTags.Protected))
        {
            return Accessibility.Protected;
        }
        if (tags.Contains(WellKnownTags.Internal))
        {
            return Accessibility.Internal;
        }
        if (tags.Contains(WellKnownTags.Private))
        {
            return Accessibility.Private;
        }
        return Accessibility.NotApplicable;
    }
}

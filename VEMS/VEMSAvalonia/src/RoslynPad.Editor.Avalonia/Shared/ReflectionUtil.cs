using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace RoslynPad.Editor;

/// <summary>
/// 反射工具类，提供高效访问私有成员的方法
/// </summary>
internal static class ReflectionUtil
{
    /// <summary>
    /// 生成访问私有字段的委托，实现高效的私有字段读取
    /// </summary>
    /// <typeparam name="TOwner">字段所属类型</typeparam>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldName">私有字段名称</param>
    /// <returns>字段访问委托</returns>
    internal static Func<TOwner, TField> GenerateGetField<TOwner, TField>(string fieldName)
    {
        var param = Parameter(typeof(TOwner));
        return Lambda<Func<TOwner, TField>>(Field(param, fieldName), param).Compile();
    }

    /// <summary>
    /// 创建访问私有方法的委托
    /// </summary>
    /// <typeparam name="TOwner">方法所属类型</typeparam>
    /// <typeparam name="TMethod">委托类型（需匹配方法签名）</typeparam>
    /// <param name="methodName">私有方法名称</param>
    /// <returns>方法调用委托</returns>
    internal static TMethod CreateDelegate<TOwner, TMethod>(string methodName)
    {
        // 获取委托的参数类型
        var args = typeof(TMethod).GetRuntimeMethods().First(c => c.Name == nameof(Action.Invoke))
            .GetParameters().Select(p => p.ParameterType).ToArray();

        // 查找匹配签名的私有方法
        var methodInfo = typeof(TOwner).GetRuntimeMethods().First(m => m.Name == methodName && m.GetParameters()
            .Select(p => p.ParameterType).SequenceEqual(args));

        // 创建委托实例
        return (TMethod)(object)methodInfo.CreateDelegate(typeof(TMethod));
    }
}

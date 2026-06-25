namespace RoslynPad.Editor;

/// <summary>
/// 扩展<see cref="IOverloadProvider"/>接口，补充重载信息刷新能力
/// </summary>
public interface IOverloadProviderEx : IOverloadProvider
{
    /// <summary>
    /// 刷新重载提供程序的内部数据，更新重载信息
    /// </summary>
    void Refresh();
}

namespace RoslynPad.Build;

/// <summary>
/// 表示库引用，可以是程序集引用、框架引用或 NuGet 包引用。
/// </summary>
/// <param name="Kind">引用类型。</param>
/// <param name="Value">引用值（路径或包 ID）。</param>
/// <param name="Version">版本号（仅用于包引用）。</param>
internal record LibraryRef(LibraryRef.RefKind Kind, string Value, string Version) : IComparable<LibraryRef>
{
    /// <summary>
    /// 创建程序集引用。
    /// </summary>
    /// <param name="path">程序集路径。</param>
    /// <returns>程序集引用实例。</returns>
    public static LibraryRef Reference(string path) => new(RefKind.Reference, path, string.Empty);

    /// <summary>
    /// 创建框架引用。
    /// </summary>
    /// <param name="id">框架引用 ID。</param>
    /// <returns>框架引用实例。</returns>
    public static LibraryRef FrameworkReference(string id) => new(RefKind.FrameworkReference, id.ToLowerInvariant(), string.Empty);

    /// <summary>
    /// 创建 NuGet 包引用。
    /// </summary>
    /// <param name="id">包 ID。</param>
    /// <param name="versionRange">版本范围。</param>
    /// <returns>包引用实例。</returns>
    public static LibraryRef PackageReference(string id, string versionRange) => new(RefKind.PackageReference, id.ToLowerInvariant(), versionRange);

    /// <summary>
    /// 比较两个库引用的顺序。
    /// </summary>
    /// <param name="other">要比较的另一个库引用。</param>
    /// <returns>比较结果。</returns>
    public int CompareTo(LibraryRef? other)
    {
        if (other == null) return 1;

        if (Kind.CompareTo(other.Kind) is var kindCompare and not 0)
        {
            return kindCompare;
        }

        if (StringComparer.Ordinal.Compare(Value, other.Value) is var valueCompare and not 0)
        {
            return valueCompare;
        }

        return StringComparer.Ordinal.Compare(Version, other.Version);
    }

    /// <summary>
    /// 引用类型枚举。
    /// </summary>
    public enum RefKind
    {
        /// <summary>程序集引用。</summary>
        Reference,
        /// <summary>框架引用。</summary>
        FrameworkReference,
        /// <summary>NuGet 包引用。</summary>
        PackageReference
    }
}

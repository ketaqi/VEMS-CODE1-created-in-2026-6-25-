using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace RoslynPad.Build;

/// <summary>
/// 提供 JSON 处理的辅助方法。
/// </summary>
internal static class JsonHelpers
{
    /// <summary>
    /// 从 <see cref="Utf8JsonReader"/> 获取值的 Span，支持跨多个段的值序列。
    /// </summary>
    /// <param name="reader">JSON 读取器。</param>
    /// <returns>包含值 Span 的可释放结构。</returns>
    /// <remarks>
    /// 如果值跨越多个缓冲区段，会从 <see cref="ArrayPool{T}"/> 租用临时数组，
    /// 调用方必须通过 <see cref="SpanDisposer.Dispose"/> 归还。
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanDisposer GetSpan(this scoped ref Utf8JsonReader reader)
    {
        if (!reader.HasValueSequence)
        {
            return new SpanDisposer(reader.ValueSpan);
        }

        var length = (int)reader.ValueSequence.Length;
        var array = ArrayPool<byte>.Shared.Rent(length);
        reader.ValueSequence.CopyTo(array);
        return new SpanDisposer(array.AsSpan(0, length), array);
    }

    /// <summary>
    /// 可释放的 Span 包装器，用于管理可能从对象池租用的缓冲区。
    /// </summary>
    /// <param name="span">只读字节 Span。</param>
    /// <param name="array">从对象池租用的数组（如果有）。</param>
    public readonly ref struct SpanDisposer(ReadOnlySpan<byte> span, byte[]? array = null)
    {
        private readonly byte[]? _array = array;

        /// <summary>
        /// 获取只读字节 Span。
        /// </summary>
        public readonly ReadOnlySpan<byte> Span = span;

        /// <summary>
        /// 归还租用的数组到对象池。
        /// </summary>
        public void Dispose()
        {
            if (_array != null)
            {
                ArrayPool<byte>.Shared.Return(_array);
            }
        }
    }
}

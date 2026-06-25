using System.Buffers;
using System.Diagnostics;

namespace RoslynPad.Build;

/// <summary>
/// 基于 <see cref="ArrayPool{T}"/> 的高性能字节缓冲区写入器。
/// </summary>
/// <remarks>
/// 使用对象池租用缓冲区以减少内存分配，在使用完毕后必须调用 <see cref="Dispose"/> 归还缓冲区。
/// </remarks>
internal sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _rentedBuffer;
    private int _index;

    private const int MinimumBufferSize = 256;

    /// <summary>
    /// 初始化 <see cref="PooledByteBufferWriter"/> 类的新实例。
    /// </summary>
    /// <param name="initialCapacity">初始缓冲区容量。</param>
    public PooledByteBufferWriter(int initialCapacity = MinimumBufferSize)
    {
        Debug.Assert(initialCapacity > 0);

        _rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        _index = 0;
    }

    /// <summary>
    /// 获取已写入的数据内存。
    /// </summary>
    public ReadOnlyMemory<byte> WrittenMemory
    {
        get
        {
            Debug.Assert(_rentedBuffer != null);
            Debug.Assert(_index <= _rentedBuffer.Length);
            return _rentedBuffer.AsMemory(0, _index);
        }
    }

    /// <summary>
    /// 获取已写入的字节数。
    /// </summary>
    public int WrittenCount
    {
        get
        {
            Debug.Assert(_rentedBuffer != null);
            return _index;
        }
    }

    /// <summary>
    /// 获取缓冲区总容量。
    /// </summary>
    public int Capacity
    {
        get
        {
            Debug.Assert(_rentedBuffer != null);
            return _rentedBuffer.Length;
        }
    }

    /// <summary>
    /// 获取剩余可用容量。
    /// </summary>
    public int FreeCapacity
    {
        get
        {
            Debug.Assert(_rentedBuffer != null);
            return _rentedBuffer.Length - _index;
        }
    }

    /// <summary>
    /// 重置写入位置，保留缓冲区。
    /// </summary>
    public void Reset() => _index = 0;

    /// <summary>
    /// 清除已写入的数据并重置写入位置。
    /// </summary>
    public void Clear() => ClearHelper();

    private void ClearHelper()
    {
        Debug.Assert(_rentedBuffer != null);
        Debug.Assert(_index <= _rentedBuffer.Length);

        _rentedBuffer.AsSpan(0, _index).Clear();
        _index = 0;
    }

    /// <summary>
    /// 清除缓冲区并归还到对象池。
    /// </summary>
    public void Dispose()
    {
        if (_rentedBuffer == null)
        {
            return;
        }

        ClearHelper();
        byte[] toReturn = _rentedBuffer;
        _rentedBuffer = null!;
        ArrayPool<byte>.Shared.Return(toReturn);
    }

    /// <summary>
    /// 推进写入位置。
    /// </summary>
    /// <param name="count">要推进的字节数。</param>
    public void Advance(int count)
    {
        Debug.Assert(_rentedBuffer != null);
        Debug.Assert(count >= 0);
        Debug.Assert(_index <= _rentedBuffer.Length - count);

        _index += count;
    }

    /// <summary>
    /// 获取可写入的内存区域。
    /// </summary>
    /// <param name="sizeHint">期望的最小大小。</param>
    /// <returns>可写入的内存区域。</returns>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return _rentedBuffer.AsMemory(_index);
    }

    /// <summary>
    /// 获取可写入的 Span 区域。
    /// </summary>
    /// <param name="sizeHint">期望的最小大小。</param>
    /// <returns>可写入的 Span 区域。</returns>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        CheckAndResizeBuffer(sizeHint);
        return _rentedBuffer.AsSpan(_index);
    }

    /// <summary>
    /// 异步将已写入的数据写入流。
    /// </summary>
    /// <param name="destination">目标流。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    internal ValueTask WriteToStreamAsync(Stream destination, CancellationToken cancellationToken) =>
        destination.WriteAsync(WrittenMemory, cancellationToken);

    /// <summary>
    /// 将已写入的数据同步写入流。
    /// </summary>
    /// <param name="destination">目标流。</param>
    internal void WriteToStream(Stream destination) => destination.Write(WrittenMemory.Span);

    /// <summary>
    /// 检查并在必要时扩展缓冲区容量。
    /// </summary>
    /// <param name="sizeHint">期望的最小可用大小。</param>
    private void CheckAndResizeBuffer(int sizeHint)
    {
        Debug.Assert(_rentedBuffer != null);
        Debug.Assert(sizeHint >= 0);

        if (sizeHint == 0)
        {
            sizeHint = MinimumBufferSize;
        }

        int availableSpace = _rentedBuffer.Length - _index;

        if (sizeHint > availableSpace)
        {
            int currentLength = _rentedBuffer.Length;
            int growBy = Math.Max(sizeHint, currentLength);

            int newSize = currentLength + growBy;

            if ((uint)newSize > int.MaxValue)
            {
                newSize = currentLength + sizeHint;
                if ((uint)newSize > int.MaxValue)
                {
                    throw new InsufficientMemoryException("BufferMaximumSizeExceeded: " + (uint)newSize);
                }
            }

            byte[] oldBuffer = _rentedBuffer;

            _rentedBuffer = ArrayPool<byte>.Shared.Rent(newSize);

            Debug.Assert(oldBuffer.Length >= _index);
            Debug.Assert(_rentedBuffer.Length >= _index);

            Span<byte> previousBuffer = oldBuffer.AsSpan(0, _index);
            previousBuffer.CopyTo(_rentedBuffer);
            previousBuffer.Clear();
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }

        Debug.Assert(_rentedBuffer.Length - _index > 0);
        Debug.Assert(_rentedBuffer.Length - _index >= sizeHint);
    }
}

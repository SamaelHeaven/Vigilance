using System.Buffers;
using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

internal ref struct SegmentedArrayProvider<T>
{
    private const int ArrayMaxLength = 0X7FFFFFC7;
    private Span<T> _currentSegment;
    private int _countInCurrentSegment;
    private readonly Span<T> _initialBuffer;
    private Segments _segments;
    private int _segmentsCount;
    private int _countInFinishedSegments;

    public int Count => checked(_countInFinishedSegments + _countInCurrentSegment);

    public SegmentedArrayProvider(in Span<T> initialBuffer)
    {
        _initialBuffer = _currentSegment = initialBuffer;
    }

    public Span<T> GetSpan()
    {
        var span = _currentSegment;
        var index = _countInCurrentSegment;
        if ((uint)index < (uint)span.Length)
            return span[index..];
        Expand();
        return _currentSegment;
    }

    public void Advance(int count)
    {
        _countInCurrentSegment += count;
    }

    private void Expand()
    {
        var currentSegmentLength = _currentSegment.Length;
        checked
        {
            _countInFinishedSegments += currentSegmentLength;
        }

        if (_countInFinishedSegments > ArrayMaxLength)
            throw new OutOfMemoryException();
        var newSegmentLength = (int)System.Math.Min(System.Math.Max(16, currentSegmentLength) * 2L, ArrayMaxLength);
        _currentSegment = _segments[_segmentsCount] = ArrayPool<T>.Shared.Rent(newSegmentLength);
        _countInCurrentSegment = 0;
        _segmentsCount++;
    }

    public void CopyToAndClear(Span<T> destination)
    {
        var segmentIndex = _segmentsCount;
        if (segmentIndex != 0)
        {
            var first = _initialBuffer;
            first.CopyTo(destination);
            destination = destination[first.Length..];
            segmentIndex--;
            if (segmentIndex != 0)
            {
                var segmentSpan = ((ReadOnlySpan<T[]>)_segments)[..segmentIndex];
                foreach (var array in segmentSpan)
                {
                    var segment = array.AsSpan();
                    segment.CopyTo(destination);
                    destination = destination[segment.Length..];
                    ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                }
            }

            var lastSegment = _segments[segmentIndex];
            lastSegment.AsSpan(0, _countInCurrentSegment).CopyTo(destination);
            ArrayPool<T>.Shared.Return(lastSegment, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
        else
        {
            _currentSegment[.._countInCurrentSegment].CopyTo(destination);
        }
    }

    [InlineArray(27)]
    private struct Segments
    {
        private T[] _element0;
    }
}

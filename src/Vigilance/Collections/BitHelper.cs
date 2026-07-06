using Vigilance.Core;

namespace Vigilance.Collections;

public readonly ref struct BitHelper
{
    private const int IntSize = sizeof(int) * 8;
    private readonly Span<int> _span;

    public BitHelper(in Span<int> span, bool clear)
    {
        if (clear)
            span.Clear();
        _span = span;
    }

    public void MarkBit(int bitPosition)
    {
        Debug.Assert(bitPosition >= 0);
        var bitArrayIndex = (uint)bitPosition / IntSize;
        var span = _span;
        if (bitArrayIndex < (uint)span.Length)
            span[(int)bitArrayIndex] |= 1 << (int)((uint)bitPosition % IntSize);
    }

    public bool IsMarked(int bitPosition)
    {
        Debug.Assert(bitPosition >= 0);
        var bitArrayIndex = (uint)bitPosition / IntSize;
        ReadOnlySpan<int> span = _span;
        return bitArrayIndex < (uint)span.Length
            && (span[(int)bitArrayIndex] & (1 << (int)((uint)bitPosition % IntSize))) != 0;
    }

    public static int ToIntArrayLength(int n)
    {
        return n > 0 ? (n - 1) / IntSize + 1 : 0;
    }
}

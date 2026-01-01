using Vigilance.Collections;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Core;

public unsafe struct IdArray : ISpanView<ulong>
{
    public const int Size = 16;
    private fixed ulong _elements[Size];

    public readonly int Count
    {
        get
        {
            for (var i = 0; i < Size; i++)
                if (_elements[i] == 0)
                    return i;
            return Size;
        }
    }

    public void Add(ulong value)
    {
        for (var i = 0; i < Size; i++)
        {
            ref var element = ref _elements[i];
            if (element != 0)
                continue;
            element = value;
            return;
        }

        throw new InvalidOperationException($"{nameof(IdArray)} is full.");
    }

    public readonly ReadOnlySpan<ulong> AsSpan()
    {
        fixed (ulong* ptr = _elements)
        {
            return new ReadOnlySpan<ulong>(ptr, Count);
        }
    }

    public readonly ValueEnumerable<FromSpan<ulong>, ulong> AsValueEnumerable()
    {
        return AsSpan().AsValueEnumerable();
    }
}

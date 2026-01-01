using System.Runtime.CompilerServices;
using Vigilance.Collections;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Core;

public struct IdArray : ISpanView<ulong>
{
    public const int Size = 16;
    private Elements _elements;

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

    public readonly unsafe ReadOnlySpan<ulong> AsSpan()
    {
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        return ((ReadOnlySpan<ulong>)_elements)[..Count];
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
    }

    public readonly ValueEnumerable<FromSpan<ulong>, ulong> AsValueEnumerable()
    {
        return AsSpan().AsValueEnumerable();
    }

    [InlineArray(Size)]
    private struct Elements
    {
        private ulong _element0;
    }
}

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vigilance.Collections;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Core;

public struct InlineIds : ISpanView<ulong>
{
    public const int Length = 16;
    private Elements _elements;

    public readonly int Count
    {
        get
        {
            for (var i = 0; i < Length; i++)
                if (_elements[i] == 0)
                    return i;
            return Length;
        }
    }

    public void Add(ulong value)
    {
        for (var i = 0; i < Length; i++)
        {
            ref var element = ref _elements[i];
            if (element != 0)
                continue;
            element = value;
            return;
        }

        throw new InvalidOperationException($"{nameof(InlineIds)} is full.");
    }

    public readonly ReadOnlySpan<ulong> AsSpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(in _elements[0], Count);
    }

    public readonly ValueEnumerable<FromSpan<ulong>, ulong> AsValueEnumerable()
    {
        return AsSpan().AsValueEnumerable();
    }

    public ValueEnumerator<FromSpan<ulong>, ulong> GetEnumerator()
    {
        return new ValueEnumerator<FromSpan<ulong>, ulong>(AsValueEnumerable().Enumerator);
    }

    [InlineArray(Length)]
    private struct Elements
    {
        private ulong _element0;
    }
}

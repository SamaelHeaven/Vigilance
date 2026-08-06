using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vigilance.Collections;

[StructLayout(LayoutKind.Sequential)]
[CollectionBuilder(typeof(InlineListBuilder), nameof(InlineListBuilder.Create))]
public struct InlineList<TStorage, TItem>
    : IList<TItem>,
        IReadOnlyList<TItem>,
        IReadOnlySpan<TItem>,
        IStructEnumerable<InlineList<TStorage, TItem>.Enumerator, TItem>
    where TStorage : struct
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private TStorage _storage;
    public int Count { get; private set; }

    public InlineList(int count)
        : this(default, count) { }

    public InlineList(in TStorage storage, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Capacity);
        _storage = storage;
        Count = count;
    }

    public TItem this[int index]
    {
        readonly get => AsReadOnlySpan()[index];
        set => AsSpan()[index] = value;
    }

    public readonly int Capacity => Unsafe.SizeOf<TStorage>() / Unsafe.SizeOf<TItem>();

    public readonly bool IsReadOnly => false;

    public void Add(in TItem item)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Count, Capacity);
        this[Count++] = item;
    }

    void ICollection<TItem>.Add(TItem item)
    {
        Add(item);
    }

    public void Clear()
    {
        AsSpan().Clear();
        Count = 0;
    }

    readonly bool ICollection<TItem>.Contains(TItem item)
    {
        return Contains(item);
    }

    public readonly bool Contains(in TItem item)
    {
        return AsReadOnlySpan().Contains(item);
    }

    public readonly void CopyTo(TItem[] array, int arrayIndex)
    {
        AsReadOnlySpan().CopyTo(array.AsSpan(arrayIndex));
    }

    public bool Remove(in TItem item)
    {
        var index = IndexOf(item);
        if (index < 0)
            return false;
        RemoveAt(index);
        return true;
    }

    bool ICollection<TItem>.Remove(TItem item)
    {
        return Remove(item);
    }

    public readonly int IndexOf(in TItem item)
    {
        return AsReadOnlySpan().IndexOf(item);
    }

    public void Insert(int index, in TItem item)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Count, Capacity);
        var span = AsSpan();
        span[index..Count].CopyTo(span[(index + 1)..]);
        span[index] = item;
        Count++;
    }

    readonly int IList<TItem>.IndexOf(TItem item)
    {
        return IndexOf(item);
    }

    void IList<TItem>.Insert(int index, TItem item)
    {
        Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
        var span = AsSpan();
        span[(index + 1)..Count].CopyTo(span[index..]);
        Count--;
        span[Count] = default!;
    }

    public Span<TItem> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<TStorage, TItem>(ref _storage), Count);
    }

    public readonly ReadOnlySpan<TItem> AsReadOnlySpan()
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<TStorage, TItem>(ref Unsafe.AsRef(in _storage)), Count);
    }

    readonly ReadOnlySpan<TItem> IReadOnlySpan<TItem>.AsSpan()
    {
        return AsReadOnlySpan();
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, TItem> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, TItem>(GetEnumerator());
    }

    readonly ValueEnumerable<StructEnumerator<Enumerator, TItem>, TItem> IStructEnumerable<
        Enumerator,
        TItem
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, TItem>(GetEnumerator());
    }

    public static implicit operator Span<TItem>(in InlineList<TStorage, TItem> list)
    {
        return Unsafe.AsRef(in list).AsSpan();
    }

    public static implicit operator ReadOnlySpan<TItem>(in InlineList<TStorage, TItem> list)
    {
        return list.AsReadOnlySpan();
    }

    public struct Enumerator : IStructEnumerator<TItem>, IValueEnumerator<TItem>
    {
        private readonly InlineList<TStorage, TItem> _list;
        private int _index;

        internal Enumerator(in InlineList<TStorage, TItem> list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_list.Count)
            {
                Current = _list[_index];
                _index++;
                return true;
            }

            Current = default!;
            _index = -1;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            Current = default!;
        }

        public TItem Current { get; private set; } = default!;

        public void Dispose() { }

        public bool TryGetNext(out TItem current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _list.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<TItem> span)
        {
            span = _list.AsReadOnlySpan();
            return true;
        }

        public bool TryCopyTo(scoped Span<TItem> destination, Index offset)
        {
            return _list.AsReadOnlySpan().TryCopyTo(destination, offset);
        }
    }
}

public static class InlineListBuilder
{
    public static InlineList<TArray, TItem> Create<TArray, TItem>(ReadOnlySpan<TItem> span)
        where TArray : struct
    {
        var list = new InlineList<TArray, TItem>(span.Length);
        span.CopyTo(list);
        return list;
    }
}

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Collections;

public struct ValueHashSet<T> : ISet<T>, IReadOnlySet<T>, IStructEnumerable<ValueHashSet<T>.Enumerator, T>
{
    private const int StackAllocThreshold = 100;
    private const int ShrinkThreshold = 3;
    private const int StartOfFreeList = -3;

    private int[]? _buckets;
    private Entry[]? _entries;
    private ulong _fastModMultiplier;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private readonly IEqualityComparer<T>? _comparer;
    private readonly IEqualityComparer<T>? _underlyingComparer;

    public ValueHashSet()
        : this(0) { }

    public ValueHashSet(IEqualityComparer<T>? comparer)
        : this(0, comparer) { }

    public ValueHashSet(int capacity, IEqualityComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > 0)
            Initialize(capacity);
        if (!typeof(T).IsValueType)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _underlyingComparer = comparer;
            if (
                typeof(T) == typeof(string)
                && NonRandomizedStringEqualityComparer.GetStringComparer(_comparer!) is { } stringComparer
            )
                _comparer = (IEqualityComparer<T>)stringComparer;
        }
        else if (comparer is not null && !ReferenceEquals(comparer, EqualityComparer<T>.Default))
        {
            _comparer = comparer;
            _underlyingComparer = comparer;
        }
    }

    public ValueHashSet(in ValueHashSet<T> source)
    {
        _comparer = source._comparer;
        _underlyingComparer = source._underlyingComparer;
        _count = source._count;
        _freeList = source._freeList;
        _freeCount = source._freeCount;
        _fastModMultiplier = source._fastModMultiplier;
        _buckets = (int[]?)source._buckets?.Clone();
        _entries = (Entry[]?)source._entries?.Clone();
    }

    public ValueHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer = null)
        : this((collection as ICollection<T>)?.Count ?? 0, comparer)
    {
        ArgumentNullException.ThrowIfNull(collection);
        UnionWith(collection);
        if (_count > 0 && _entries!.Length / _count > ShrinkThreshold)
            TrimExcess();
    }

    public readonly IEqualityComparer<T> Comparer => _underlyingComparer ?? EqualityComparer<T>.Default;

    public readonly int Count => _count - _freeCount;

    public readonly int Capacity => _entries?.Length ?? 0;

    readonly bool ICollection<T>.IsReadOnly => false;

    public bool Add(in T item)
    {
        return AddIfNotPresent(item, out _);
    }

    public void Clear()
    {
        var count = _count;
        if (count <= 0)
            return;
        Debug.Assert(_buckets != null);
        Debug.Assert(_entries != null);
        Array.Clear(_buckets);
        _count = 0;
        _freeList = -1;
        _freeCount = 0;
        Array.Clear(_entries, 0, count);
    }

    public readonly bool Contains(in T item)
    {
        return FindItemIndex(item) >= 0;
    }

    public bool Remove(in T item)
    {
        if (_buckets == null)
            return false;
        Debug.Assert(_entries != null);
        var entries = _entries;
        uint collisionCount = 0;
        var last = -1;
        var comparer = _comparer;
        Debug.Assert(typeof(T).IsValueType || comparer is not null);
        var hashCode = (uint)(
            typeof(T).IsValueType && comparer == null ? item!.GetHashCode()
            : item is null ? 0
            : comparer!.GetHashCode(item)
        );
        ref var bucket = ref GetBucket(hashCode);
        var i = bucket - 1;
        while (i >= 0)
        {
            ref var entry = ref entries[i];
            if (
                entry.HashCode == hashCode
                && (
                    typeof(T).IsValueType && comparer == null
                        ? EqualityComparer<T>.Default.Equals(entry.Value, item)
                        : comparer!.Equals(entry.Value, item)
                )
            )
            {
                if (last < 0)
                    bucket = entry.Next + 1;
                else
                    entries[last].Next = entry.Next;
                Debug.Assert(StartOfFreeList - _freeList < 0);
                entry.Next = StartOfFreeList - _freeList;
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    entry.Value = default!;
                _freeList = i;
                _freeCount++;
                return true;
            }

            last = i;
            i = entry.Next;
            collisionCount++;
            if (collisionCount > (uint)entries.Length)
                ThrowConcurrentOperation();
        }

        return false;
    }

    public readonly bool TryGetValue(in T equalValue, [MaybeNullWhen(false)] out T actualValue)
    {
        if (_buckets != null)
        {
            var index = FindItemIndex(equalValue);
            if (index >= 0)
            {
                actualValue = _entries![index].Value;
                return true;
            }
        }

        actualValue = default;
        return false;
    }

    public void UnionWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (var item in other)
            AddIfNotPresent(item, out _);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return;
        if (other is ICollection<T> { Count: 0 })
        {
            Clear();
            return;
        }

        IntersectWithEnumerable(other);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return;
        foreach (var element in other)
            Remove(element);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
        {
            UnionWith(other);
            return;
        }

        SymmetricExceptWithEnumerable(other);
    }

    public readonly bool IsSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return true;
        if (other is ICollection<T> otherAsCollection && Count > otherAsCollection.Count)
            return false;
        var (uniqueCount, unfoundCount) = CheckUniqueAndUnfoundElements(other, false);
        return uniqueCount == Count && unfoundCount >= 0;
    }

    public readonly bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is ICollection<T> otherAsCollection)
        {
            if (otherAsCollection.Count <= Count)
                return false;
            if (Count == 0)
                return true;
        }

        var (uniqueCount, unfoundCount) = CheckUniqueAndUnfoundElements(other, false);
        return uniqueCount == Count && unfoundCount > 0;
    }

    public readonly bool IsSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is ICollection<T> { Count: 0 })
            return true;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var element in other)
            if (!Contains(element))
                return false;
        return true;
    }

    public readonly bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return false;
        if (other is ICollection<T> { Count: 0 })
            return true;
        var (uniqueCount, unfoundCount) = CheckUniqueAndUnfoundElements(other, true);
        return uniqueCount < Count && unfoundCount == 0;
    }

    public readonly bool Overlaps(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var element in other)
            if (Contains(element))
                return true;
        return false;
    }

    public readonly bool SetEquals(IEnumerable<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is ICollection<T> otherAsCollection)
        {
            if (Count == 0)
                return otherAsCollection.Count == 0;
            if (Count > otherAsCollection.Count)
                return false;
        }

        var (uniqueCount, unfoundCount) = CheckUniqueAndUnfoundElements(other, true);
        return uniqueCount == Count && unfoundCount == 0;
    }

    public int RemoveWhere(Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        var entries = _entries;
        var numRemoved = 0;
        for (var i = 0; i < _count; i++)
        {
            ref var entry = ref entries![i];
            if (entry.Next < -1)
                continue;
            var value = entry.Value;
            if (match(value) && Remove(value))
                numRemoved++;
        }

        return numRemoved;
    }

    public readonly void CopyTo(T[] array)
    {
        CopyTo(array.AsSpan(), 0, Count);
    }

    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex, Count);
    }

    public readonly void CopyTo(in Span<T> span, int arrayIndex = 0)
    {
        CopyTo(span, arrayIndex, Count);
    }

    public readonly void CopyTo(in Span<T> span, int arrayIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (arrayIndex > span.Length || count > span.Length - arrayIndex)
            throw new ArgumentException("Destination array is not long enough.", nameof(span));
        var entries = _entries;
        for (var i = 0; i < _count && count != 0; i++)
        {
            ref var entry = ref entries![i];
            if (entry.Next < -1)
                continue;
            span[arrayIndex++] = entry.Value;
            count--;
        }
    }

    public readonly void CopyTo(ref ValueHashSet<T> hashSet)
    {
        hashSet.Clear();
        hashSet.EnsureCapacity(Count);
        foreach (var item in this)
            hashSet.Add(item);
    }

    public int EnsureCapacity(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        var currentCapacity = _entries?.Length ?? 0;
        if (currentCapacity >= capacity)
            return currentCapacity;
        if (_buckets == null)
            return Initialize(capacity);
        var newSize = HashHelpers.GetPrime(capacity);
        Resize(newSize);
        return newSize;
    }

    public void TrimExcess()
    {
        TrimExcess(Count);
    }

    public void TrimExcess(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, Count);
        var newSize = HashHelpers.GetPrime(capacity);
        var oldEntries = _entries;
        var currentCapacity = oldEntries?.Length ?? 0;
        if (newSize >= currentCapacity)
            return;
        var oldCount = _count;
        Initialize(newSize);
        Debug.Assert(oldEntries is not null);
        var entries = _entries;
        var count = 0;
        for (var i = 0; i < oldCount; i++)
        {
            var hashCode = oldEntries[i].HashCode;
            if (oldEntries[i].Next < -1)
                continue;
            ref var entry = ref entries![count];
            entry = oldEntries[i];
            ref var bucket = ref GetBucket(hashCode);
            entry.Next = bucket - 1;
            bucket = count + 1;
            count++;
        }

        _count = count;
        _freeCount = 0;
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, T> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, T>(GetEnumerator());
    }

    readonly ValueEnumerable<StructEnumerator<Enumerator, T>, T> IStructEnumerable<Enumerator, T>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, T>(GetEnumerator());
    }

    internal readonly int FindItemIndex(in T item)
    {
        var buckets = _buckets;
        if (buckets == null)
            return -1;
        var entries = _entries;
        Debug.Assert(entries != null);
        uint collisionCount = 0;
        var comparer = _comparer;
        if (typeof(T).IsValueType && comparer == null)
        {
            var hashCode = (uint)item!.GetHashCode();
            var i = GetBucket(hashCode) - 1;
            while (i >= 0)
            {
                ref var entry = ref entries[i];
                if (entry.HashCode == hashCode && EqualityComparer<T>.Default.Equals(entry.Value, item))
                    return i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            var hashCode = (uint)(item is null ? 0 : comparer.GetHashCode(item));
            var i = GetBucket(hashCode) - 1;
            while (i >= 0)
            {
                ref var entry = ref entries[i];
                if (entry.HashCode == hashCode && comparer.Equals(entry.Value, item))
                    return i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }

        return -1;
    }

    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var buckets = new int[size];
        var entries = new Entry[size];
        _freeList = -1;
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)size);
        _buckets = buckets;
        _entries = entries;
        return size;
    }

    private bool AddIfNotPresent(in T value, out int location)
    {
        if (_buckets == null)
            Initialize(0);
        Debug.Assert(_buckets != null);
        var entries = _entries;
        Debug.Assert(entries != null);
        var comparer = _comparer;
        uint collisionCount = 0;
        ref var bucket = ref Unsafe.NullRef<int>();
        uint hashCode;
        if (typeof(T).IsValueType && comparer == null)
        {
            hashCode = (uint)value!.GetHashCode();
            bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref entries[i];
                if (entry.HashCode == hashCode && EqualityComparer<T>.Default.Equals(entry.Value, value))
                {
                    location = i;
                    return false;
                }

                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }
        else
        {
            Debug.Assert(comparer is not null);
            hashCode = (uint)(value is null ? 0 : comparer.GetHashCode(value));
            bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref entries[i];
                if (entry.HashCode == hashCode && comparer.Equals(entry.Value, value))
                {
                    location = i;
                    return false;
                }

                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)entries.Length)
                    ThrowConcurrentOperation();
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            Debug.Assert(StartOfFreeList - entries[_freeList].Next >= -1);
            _freeList = StartOfFreeList - entries[_freeList].Next;
            _freeCount--;
        }
        else
        {
            var count = _count;
            if (count == entries.Length)
            {
                Resize();
                bucket = ref GetBucket(hashCode);
            }

            index = count;
            _count = count + 1;
            entries = _entries;
        }

        {
            ref var entry = ref entries![index];
            entry.HashCode = hashCode;
            entry.Next = bucket - 1;
            entry.Value = value;
            bucket = index + 1;
            location = index;
        }

        return true;
    }

    private void Resize()
    {
        Resize(HashHelpers.ExpandPrime(_count));
    }

    private void Resize(int newSize)
    {
        Debug.Assert(_entries != null);
        Debug.Assert(newSize >= _entries.Length);
        var entries = new Entry[newSize];
        var count = _count;
        Array.Copy(_entries, entries, count);
        _buckets = new int[newSize];
        _fastModMultiplier = HashHelpers.GetFastModMultiplier((uint)newSize);
        for (var i = 0; i < count; i++)
            if (entries[i].Next >= -1)
            {
                ref var bucket = ref GetBucket(entries[i].HashCode);
                entries[i].Next = bucket - 1;
                bucket = i + 1;
            }

        _entries = entries;
    }

    private void IntersectWithEnumerable(IEnumerable<T> other)
    {
        Debug.Assert(_buckets != null);
        var originalCount = _count;
        var intArrayLength = BitHelper.ToIntArrayLength(originalCount);
        Span<int> span = stackalloc int[StackAllocThreshold];
        var bitHelper =
            (uint)intArrayLength <= StackAllocThreshold
                ? new BitHelper(span[..intArrayLength], true)
                : new BitHelper(new int[intArrayLength], false);
        foreach (var item in other)
        {
            var index = FindItemIndex(item);
            if (index >= 0)
                bitHelper.MarkBit(index);
        }

        for (var i = 0; i < originalCount; i++)
        {
            ref var entry = ref _entries![i];
            if (entry.Next >= -1 && !bitHelper.IsMarked(i))
                Remove(entry.Value);
        }
    }

    private void SymmetricExceptWithEnumerable(IEnumerable<T> other)
    {
        var originalCount = _count;
        var intArrayLength = BitHelper.ToIntArrayLength(originalCount);
        Span<int> itemsToRemoveSpan = stackalloc int[StackAllocThreshold / 2];
        var itemsToRemove =
            intArrayLength <= StackAllocThreshold / 2
                ? new BitHelper(itemsToRemoveSpan[..intArrayLength], true)
                : new BitHelper(new int[intArrayLength], false);
        Span<int> itemsAddedFromOtherSpan = stackalloc int[StackAllocThreshold / 2];
        var itemsAddedFromOther =
            intArrayLength <= StackAllocThreshold / 2
                ? new BitHelper(itemsAddedFromOtherSpan[..intArrayLength], true)
                : new BitHelper(new int[intArrayLength], false);
        foreach (var item in other)
            if (AddIfNotPresent(item, out var location))
                itemsAddedFromOther.MarkBit(location);
            else if (location < originalCount && !itemsAddedFromOther.IsMarked(location))
                itemsToRemove.MarkBit(location);

        for (var i = 0; i < originalCount; i++)
            if (itemsToRemove.IsMarked(i))
                Remove(_entries![i].Value);
    }

    private readonly (int UniqueCount, int UnfoundCount) CheckUniqueAndUnfoundElements(
        IEnumerable<T> other,
        bool returnIfUnfound
    )
    {
        if (_count == 0)
        {
            var numElementsInOther = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var unused in other)
            {
                numElementsInOther++;
                break;
            }

            return (0, numElementsInOther);
        }

        Debug.Assert(_buckets != null && _count > 0);
        var originalCount = _count;
        var intArrayLength = BitHelper.ToIntArrayLength(originalCount);
        Span<int> span = stackalloc int[StackAllocThreshold];
        var bitHelper =
            intArrayLength <= StackAllocThreshold
                ? new BitHelper(span[..intArrayLength], true)
                : new BitHelper(new int[intArrayLength], false);
        var unfoundCount = 0;
        var uniqueFoundCount = 0;
        foreach (var item in other)
        {
            var index = FindItemIndex(item);
            if (index >= 0)
            {
                if (bitHelper.IsMarked(index))
                    continue;
                bitHelper.MarkBit(index);
                uniqueFoundCount++;
            }
            else
            {
                unfoundCount++;
                if (returnIfUnfound)
                    break;
            }
        }

        return (uniqueFoundCount, unfoundCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucket(uint hashCode)
    {
        var buckets = _buckets!;
        return ref buckets[HashHelpers.FastMod(hashCode, (uint)buckets.Length, _fastModMultiplier)];
    }

    [DoesNotReturn]
    private static void ThrowConcurrentOperation()
    {
        throw new InvalidOperationException(
            "Operations that change non-concurrent collections must have exclusive access."
        );
    }

    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    bool ISet<T>.Add(T item)
    {
        return Add(item);
    }

    readonly bool ICollection<T>.Contains(T item)
    {
        return Contains(item);
    }

    readonly bool IReadOnlySet<T>.Contains(T item)
    {
        return Contains(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        return Remove(item);
    }

    readonly void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        CopyTo(array, arrayIndex);
    }

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private struct Entry
    {
        public uint HashCode;
        public int Next;
        public T Value;
    }

    public struct Enumerator : IStructEnumerator<T>, IValueEnumerator<T>
    {
        private readonly ValueHashSet<T> _hashSet;
        private int _index;

        internal Enumerator(in ValueHashSet<T> hashSet)
        {
            _hashSet = hashSet;
            _index = 0;
            Current = default!;
        }

        public bool MoveNext()
        {
            while ((uint)_index < (uint)_hashSet._count)
            {
                ref var entry = ref _hashSet._entries![_index++];
                if (entry.Next < -1)
                    continue;
                Current = entry.Value;
                return true;
            }

            _index = _hashSet._count + 1;
            Current = default!;
            return false;
        }

        public T Current { get; private set; }

        public void Reset()
        {
            _index = 0;
            Current = default!;
        }

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _hashSet.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            return false;
        }
    }
}

public static class ValueHashSetExtensions
{
    extension<T>(in ValueHashSet<T> hashSet)
    {
        public ValueHashSet<T> ToValueHashSet()
        {
            return new ValueHashSet<T>(in hashSet);
        }
    }

    extension<T>(IEnumerable<T> enumerable)
    {
        public ValueHashSet<T> ToValueHashSet(IEqualityComparer<T>? comparer = null)
        {
            return new ValueHashSet<T>(enumerable, comparer);
        }
    }

    extension<TEnumerator, T>(in ValueEnumerable<TEnumerator, T> enumerable)
        where TEnumerator : struct, IValueEnumerator<T>, allows ref struct
    {
        public ValueHashSet<T> ToValueHashSet(IEqualityComparer<T>? comparer = null)
        {
            using var enumerator = enumerable.Enumerator;
            var result = new ValueHashSet<T>(comparer);
            if (enumerator.TryGetNonEnumeratedCount(out var count))
                result.EnsureCapacity(count);
            while (enumerator.TryGetNext(out var item))
                result.Add(item);
            return result;
        }
    }
}

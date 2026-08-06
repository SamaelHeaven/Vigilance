using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

public struct ValueEntitySparseSet
    : ISparseSet<Entity>,
        ISet<Entity>,
        IReadOnlySet<Entity>,
        IReadOnlyList<Entity>,
        IStructEnumerable<ValueEntitySparseSet.Enumerator, Entity>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueSparseSet<EntityId> _sparseSet;

    public ValueEntitySparseSet(Scene scene, int sparseChunkSize = DefaultSparseChunkSize)
    {
        _sparseSet = new ValueSparseSet<EntityId>(id => id.Index, sparseChunkSize);
        Scene = scene;
    }

    public Scene Scene { get; }

    public readonly int Count => _sparseSet.Count;

    public readonly Entity this[int index] => new(_sparseSet[index], Scene);

    [UnscopedRef]
    public readonly ValueListView<EntityId> Keys => _sparseSet.Keys;

    readonly bool ICollection<Entity>.IsReadOnly => false;

    public bool Add(in Entity key)
    {
        return _sparseSet.Add(key.Id);
    }

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public readonly bool Contains(in Entity key)
    {
        return _sparseSet.Contains(key.Id);
    }

    public bool Remove(in Entity key)
    {
        return _sparseSet.Remove(key.Id);
    }

    public readonly int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key.Id);
    }

    public void UnionWith(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        foreach (var item in other)
            Add(item);
    }

    public void IntersectWith(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return;
        if (other is ICollection<Entity> { Count: 0 })
        {
            Clear();
            return;
        }

        var otherSet = other.ToValueHashSet();
        for (var i = Count - 1; i >= 0; i--)
            if (!otherSet.Contains(this[i]))
                Remove(this[i]);
    }

    public void ExceptWith(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return;
        foreach (var item in other)
            Remove(item);
    }

    public void SymmetricExceptWith(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
        {
            UnionWith(other);
            return;
        }

        var otherSet = other.ToValueHashSet();
        foreach (var item in otherSet)
            if (!Remove(item))
                Add(item);
    }

    public readonly bool IsSubsetOf(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return true;
        if (other is ICollection<Entity> otherAsCollection && Count > otherAsCollection.Count)
            return false;
        var otherSet = other.ToValueHashSet();
        return Count <= otherSet.Count && IsContainedIn(otherSet);
    }

    public readonly bool IsProperSubsetOf(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is ICollection<Entity> { Count: 0 })
            return false;
        var otherSet = other.ToValueHashSet();
        return Count < otherSet.Count && IsContainedIn(otherSet);
    }

    public readonly bool IsSupersetOf(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (other is ICollection<Entity> { Count: 0 })
            return true;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in other)
            if (!Contains(item))
                return false;
        return true;
    }

    public readonly bool IsProperSupersetOf(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return false;
        var otherSet = other.ToValueHashSet();
        return otherSet.Count < Count && ContainsAll(otherSet);
    }

    public readonly bool Overlaps(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Count == 0)
            return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in other)
            if (Contains(item))
                return true;
        return false;
    }

    public readonly bool SetEquals(IEnumerable<Entity> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        var otherSet = other.ToValueHashSet();
        return otherSet.Count == Count && ContainsAll(otherSet);
    }

    public readonly void CopyTo(Entity[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan());
    }

    public readonly void CopyTo(in Span<Entity> span, int arrayIndex = 0)
    {
        if ((uint)arrayIndex > (uint)span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(span));
        for (var i = 0; i < Count; i++)
            span[arrayIndex + i] = this[i];
    }

    public readonly void CopyTo(ref ValueEntitySparseSet sparseSet)
    {
        sparseSet.Clear();
        for (var i = 0; i < Count; i++)
            sparseSet.Add(this[i]);
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, Entity> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, Entity>(GetEnumerator());
    }

    readonly ValueEnumerable<StructEnumerator<Enumerator, Entity>, Entity> IStructEnumerable<
        Enumerator,
        Entity
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, Entity>(GetEnumerator());
    }

    void ICollection<Entity>.Add(Entity item)
    {
        Add(item);
    }

    bool ISet<Entity>.Add(Entity item)
    {
        return Add(item);
    }

    readonly bool ICollection<Entity>.Contains(Entity item)
    {
        return Contains(item);
    }

    readonly bool IReadOnlySet<Entity>.Contains(Entity item)
    {
        return Contains(item);
    }

    bool ICollection<Entity>.Remove(Entity item)
    {
        return Remove(item);
    }

    readonly void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    private readonly bool IsContainedIn(in ValueHashSet<Entity> other)
    {
        for (var i = 0; i < Count; i++)
            if (!other.Contains(this[i]))
                return false;
        return true;
    }

    private readonly bool ContainsAll(in ValueHashSet<Entity> other)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in other)
            if (!Contains(item))
                return false;
        return true;
    }

    public struct Enumerator : IStructEnumerator<Entity>, IValueEnumerator<Entity>
    {
        private readonly ValueEntitySparseSet _sparseSet;
        private int _index;

        internal Enumerator(in ValueEntitySparseSet sparseSet)
        {
            _sparseSet = sparseSet;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_sparseSet.Count)
            {
                Current = _sparseSet[_index];
                _index++;
                return true;
            }

            Current = default;
            _index = -1;
            return false;
        }

        public Entity Current { get; private set; }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public void Dispose() { }

        public bool TryGetNext(out Entity current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _sparseSet.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<Entity> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<Entity> destination, Index offset)
        {
            return false;
        }
    }
}

public struct ValueEntitySparseSet<TValue>
    : ISparseSet<Entity, TValue, ValueList<TValue>>,
        IDictionary<Entity, TValue>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>,
        IStructEnumerable<ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator, KeyValuePair<Entity, TValue>>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueEntitySparseSet<TValue, ValueList<TValue>> _sparseSet;

    public ValueEntitySparseSet(Scene scene, int sparseChunkSize = DefaultSparseChunkSize)
    {
        _sparseSet = new ValueEntitySparseSet<TValue, ValueList<TValue>>(scene, [], sparseChunkSize);
    }

    [UnscopedRef]
    internal ref ValueEntitySparseSet<TValue, ValueList<TValue>> Storage => ref _sparseSet;

    public readonly Scene Scene => _sparseSet.Scene;

    public readonly ISparseSet<TValue, ValueList<TValue>>.ValueEnumerable Values => _sparseSet.Values;

    public readonly ValueEntitySparseSet<TValue, ValueList<TValue>>.KeyEnumerable Keys => _sparseSet.Keys;

    public readonly int Count => _sparseSet.Count;

    public TValue this[in Entity key]
    {
        readonly get => _sparseSet[key];
        set => _sparseSet[key] = value;
    }

    public readonly KeyValuePair<Entity, TValue> this[int index] => _sparseSet[index];

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public readonly bool ContainsKey(in Entity key)
    {
        return _sparseSet.ContainsKey(key);
    }

    public readonly bool TryGetValue(in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key, out value);
    }

    public readonly TValue? GetValueOrDefault(in Entity key)
    {
        return _sparseSet.GetValueOrDefault(key);
    }

    public readonly TValue GetValueOrDefault(in Entity key, in TValue defaultValue)
    {
        return _sparseSet.GetValueOrDefault(key, defaultValue);
    }

    public bool Remove(in Entity key)
    {
        return _sparseSet.Remove(key);
    }

    public readonly int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public readonly void CopyTo(in Span<KeyValuePair<Entity, TValue>> span, int arrayIndex = 0)
    {
        _sparseSet.CopyTo(span, arrayIndex);
    }

    public readonly void CopyTo(ref ValueEntitySparseSet<TValue> sparseSet)
    {
        _sparseSet.CopyTo(ref sparseSet._sparseSet);
    }

    public readonly ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator GetEnumerator()
    {
        return _sparseSet.GetEnumerator();
    }

    public readonly ValueEnumerable<
        ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator,
        KeyValuePair<Entity, TValue>
    > AsValueEnumerable()
    {
        return _sparseSet.AsValueEnumerable();
    }

    readonly ValueEnumerable<
        StructEnumerator<ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator, KeyValuePair<Entity, TValue>>,
        KeyValuePair<Entity, TValue>
    > IStructEnumerable<
        ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator,
        KeyValuePair<Entity, TValue>
    >.AsValueEnumerable()
    {
        return new StructEnumerator<
            ValueEntitySparseSet<TValue, ValueList<TValue>>.Enumerator,
            KeyValuePair<Entity, TValue>
        >(GetEnumerator());
    }

    void ICollection<KeyValuePair<Entity, TValue>>.Add(KeyValuePair<Entity, TValue> item)
    {
        ((ICollection<KeyValuePair<Entity, TValue>>)_sparseSet).Add(item);
    }

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.Contains(KeyValuePair<Entity, TValue> item)
    {
        return ((ICollection<KeyValuePair<Entity, TValue>>)_sparseSet).Contains(item);
    }

    readonly void ICollection<KeyValuePair<Entity, TValue>>.CopyTo(KeyValuePair<Entity, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.Remove(KeyValuePair<Entity, TValue> item)
    {
        return TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);
    }

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.IsReadOnly => false;

    void IDictionary<Entity, TValue>.Add(Entity key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        this[key] = value;
    }

    readonly bool IDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    bool IDictionary<Entity, TValue>.Remove(Entity key)
    {
        return Remove(key);
    }

    readonly bool IDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IDictionary<Entity, TValue>.this[Entity key]
    {
        readonly get => this[key];
        set => this[key] = value;
    }

    readonly ICollection<TValue> IDictionary<Entity, TValue>.Values => ((IDictionary<Entity, TValue>)_sparseSet).Values;

    readonly ICollection<Entity> IDictionary<Entity, TValue>.Keys => ((IDictionary<Entity, TValue>)_sparseSet).Keys;

    readonly IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

    readonly IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values =>
        ((IReadOnlyDictionary<Entity, TValue>)_sparseSet).Values;

    readonly bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    readonly TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    readonly bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }
}

public struct ValueEntitySparseSet<TValue, TStorage>
    : ISparseSet<Entity, TValue, TStorage>,
        IDictionary<Entity, TValue>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>,
        IStructEnumerable<ValueEntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueSparseSet<EntityId, TValue, TStorage> _sparseSet;

    public ValueEntitySparseSet(Scene scene, in TStorage storage, int sparseChunkSize = DefaultSparseChunkSize)
    {
        _sparseSet = new ValueSparseSet<EntityId, TValue, TStorage>(storage, id => id.Index, sparseChunkSize);
        Scene = scene;
    }

    public Scene Scene { get; }

    public readonly ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public readonly KeyEnumerable Keys => new(this);

    public readonly int Count => _sparseSet.Count;

    public TValue this[in Entity key]
    {
        readonly get => _sparseSet[key.Id];
        set => _sparseSet[key.Id] = value;
    }

    public readonly KeyValuePair<Entity, TValue> this[int index]
    {
        get
        {
            var pair = _sparseSet[index];
            return new KeyValuePair<Entity, TValue>(new Entity(pair.Key, Scene), pair.Value);
        }
    }

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public readonly bool ContainsKey(in Entity key)
    {
        return _sparseSet.ContainsKey(key.Id);
    }

    public readonly bool TryGetValue(in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key.Id, out value);
    }

    public readonly TValue? GetValueOrDefault(in Entity key)
    {
        return _sparseSet.GetValueOrDefault(key.Id);
    }

    public readonly TValue GetValueOrDefault(in Entity key, in TValue defaultValue)
    {
        return _sparseSet.GetValueOrDefault(key.Id, defaultValue);
    }

    public bool Remove(in Entity key)
    {
        return _sparseSet.Remove(key.Id);
    }

    public readonly int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key.Id);
    }

    public readonly void CopyTo(in Span<KeyValuePair<Entity, TValue>> span, int arrayIndex = 0)
    {
        if ((uint)arrayIndex > (uint)span.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (span.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(span));
        for (var i = 0; i < Count; i++)
            span[arrayIndex + i] = this[i];
    }

    public readonly void CopyTo(ref ValueEntitySparseSet<TValue, TStorage> sparseSet)
    {
        sparseSet.Clear();
        for (var i = 0; i < Count; i++)
        {
            var pair = this[i];
            sparseSet[pair.Key] = pair.Value;
        }
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public readonly ValueEnumerable<Enumerator, KeyValuePair<Entity, TValue>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, KeyValuePair<Entity, TValue>>(GetEnumerator());
    }

    readonly ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<Entity, TValue>>,
        KeyValuePair<Entity, TValue>
    > IStructEnumerable<Enumerator, KeyValuePair<Entity, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, KeyValuePair<Entity, TValue>>(GetEnumerator());
    }

    void ICollection<KeyValuePair<Entity, TValue>>.Add(KeyValuePair<Entity, TValue> item)
    {
        if (ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        this[item.Key] = item.Value;
    }

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.Contains(KeyValuePair<Entity, TValue> item)
    {
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    readonly void ICollection<KeyValuePair<Entity, TValue>>.CopyTo(KeyValuePair<Entity, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        CopyTo(array.AsSpan(), arrayIndex);
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.Remove(KeyValuePair<Entity, TValue> item)
    {
        return TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);
    }

    readonly bool ICollection<KeyValuePair<Entity, TValue>>.IsReadOnly => false;

    void IDictionary<Entity, TValue>.Add(Entity key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        this[key] = value;
    }

    readonly bool IDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    bool IDictionary<Entity, TValue>.Remove(Entity key)
    {
        return Remove(key);
    }

    readonly bool IDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IDictionary<Entity, TValue>.this[Entity key]
    {
        readonly get => this[key];
        set => this[key] = value;
    }

    readonly ICollection<TValue> IDictionary<Entity, TValue>.Values => _sparseSet.Values;

    readonly ICollection<Entity> IDictionary<Entity, TValue>.Keys => Keys;

    readonly IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => Keys;

    readonly IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => _sparseSet.Values;

    readonly bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    readonly TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    readonly bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    public struct Enumerator
        : IStructEnumerator<KeyValuePair<Entity, TValue>>,
            IValueEnumerator<KeyValuePair<Entity, TValue>>
    {
        private readonly ValueEntitySparseSet<TValue, TStorage> _sparseSet;
        private int _index;

        internal Enumerator(in ValueEntitySparseSet<TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_sparseSet.Count)
            {
                Current = _sparseSet[_index];
                _index++;
                return true;
            }

            Current = default!;
            _index = -1;
            return false;
        }

        public KeyValuePair<Entity, TValue> Current { get; private set; }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public void Dispose() { }

        public bool TryGetNext(out KeyValuePair<Entity, TValue> current)
        {
            Unsafe.SkipInit(out current);
            var result = MoveNext();
            if (result)
                current = Current;
            return result;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _sparseSet.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<KeyValuePair<Entity, TValue>> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<KeyValuePair<Entity, TValue>> destination, Index offset)
        {
            return false;
        }
    }

    public readonly struct KeyEnumerable
        : IReadOnlyList<Entity>,
            ICollection<Entity>,
            IStructEnumerable<KeyEnumerable.Enumerator, Entity>
    {
        private readonly ValueEntitySparseSet<TValue, TStorage> _sparseSet;

        void ICollection<Entity>.Add(Entity item)
        {
            throw new NotSupportedException($"{nameof(KeyEnumerable)} is read-only.");
        }

        void ICollection<Entity>.Clear()
        {
            throw new NotSupportedException($"{nameof(KeyEnumerable)} is read-only.");
        }

        public bool Contains(Entity item)
        {
            return _sparseSet.ContainsKey(item);
        }

        public void CopyTo(Entity[] array, int arrayIndex)
        {
            AsValueEnumerable().CopyTo(array.AsSpan(arrayIndex));
        }

        bool ICollection<Entity>.Remove(Entity item)
        {
            throw new NotSupportedException($"{nameof(KeyEnumerable)} is read-only.");
        }

        internal KeyEnumerable(in ValueEntitySparseSet<TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
        }

        public int Count => _sparseSet.Count;

        bool ICollection<Entity>.IsReadOnly => true;

        public Entity this[int index] => new(_sparseSet._sparseSet.Keys[index], _sparseSet.Scene);

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_sparseSet);
        }

        public ValueEnumerable<Enumerator, Entity> AsValueEnumerable()
        {
            return new ValueEnumerable<Enumerator, Entity>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<Enumerator, Entity>, Entity> IStructEnumerable<
            Enumerator,
            Entity
        >.AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, Entity>(GetEnumerator());
        }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public struct Enumerator : IStructEnumerator<Entity>, IValueEnumerator<Entity>
        {
            private readonly ValueEntitySparseSet<TValue, TStorage> _sparseSet;
            private int _index;

            internal Enumerator(in ValueEntitySparseSet<TValue, TStorage> sparseSet)
            {
                _sparseSet = sparseSet;
                Reset();
            }

            public bool MoveNext()
            {
                if ((uint)_index < (uint)_sparseSet.Count)
                {
                    Current = new Entity(_sparseSet._sparseSet.Keys[_index], _sparseSet.Scene);
                    _index++;
                    return true;
                }

                Current = default;
                _index = -1;
                return false;
            }

            public Entity Current { get; private set; }

            public void Reset()
            {
                _index = 0;
                Current = default;
            }

            public void Dispose() { }

            public bool TryGetNext(out Entity current)
            {
                Unsafe.SkipInit(out current);
                var result = MoveNext();
                if (result)
                    current = Current;
                return result;
            }

            public bool TryGetNonEnumeratedCount(out int count)
            {
                count = _sparseSet.Count;
                return true;
            }

            public bool TryGetSpan(out ReadOnlySpan<Entity> span)
            {
                span = default;
                return false;
            }

            public bool TryCopyTo(scoped Span<Entity> destination, Index offset)
            {
                return false;
            }
        }
    }
}

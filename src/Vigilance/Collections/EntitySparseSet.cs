using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Vigilance.Collections;

public class EntitySparseSet
    : ISparseSet<Entity>,
        ISet<Entity>,
        IReadOnlySet<Entity>,
        IReadOnlyList<Entity>,
        IStructEnumerable<EntitySparseSet.Enumerator, Entity>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueEntitySparseSet _sparseSet;

    public EntitySparseSet(Scene scene, int sparseChunkSize = DefaultSparseChunkSize)
    {
        _sparseSet = new ValueEntitySparseSet(scene, sparseChunkSize);
    }

    public Scene Scene => _sparseSet.Scene;

    public ValueListView<EntityId> Keys => _sparseSet.Keys;

    bool IReadOnlySet<Entity>.Contains(Entity item)
    {
        return _sparseSet.Contains(item);
    }

    bool ICollection<Entity>.IsReadOnly => false;

    public void UnionWith(IEnumerable<Entity> other)
    {
        _sparseSet.UnionWith(other);
    }

    public void IntersectWith(IEnumerable<Entity> other)
    {
        _sparseSet.IntersectWith(other);
    }

    public void ExceptWith(IEnumerable<Entity> other)
    {
        _sparseSet.ExceptWith(other);
    }

    public void SymmetricExceptWith(IEnumerable<Entity> other)
    {
        _sparseSet.SymmetricExceptWith(other);
    }

    public bool IsSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSubsetOf(other);
    }

    public bool IsProperSubsetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<Entity> other)
    {
        return _sparseSet.IsProperSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<Entity> other)
    {
        return _sparseSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<Entity> other)
    {
        return _sparseSet.SetEquals(other);
    }

    void ICollection<Entity>.Add(Entity item)
    {
        _sparseSet.Add(item);
    }

    bool ISet<Entity>.Add(Entity item)
    {
        return _sparseSet.Add(item);
    }

    bool ICollection<Entity>.Contains(Entity item)
    {
        return _sparseSet.Contains(item);
    }

    bool ICollection<Entity>.Remove(Entity item)
    {
        return _sparseSet.Remove(item);
    }

    void ICollection<Entity>.CopyTo(Entity[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        _sparseSet.CopyTo(array.AsSpan(), arrayIndex);
    }

    public int Count => _sparseSet.Count;

    public Entity this[int index] => _sparseSet[index];

    public bool Add(in Entity key)
    {
        return _sparseSet.Add(key);
    }

    public void Clear()
    {
        _sparseSet.Clear();
    }

    public bool Contains(in Entity key)
    {
        return _sparseSet.Contains(key);
    }

    public bool Remove(in Entity key)
    {
        return _sparseSet.Remove(key);
    }

    public int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    ValueEnumerable<StructEnumerator<Enumerator, Entity>, Entity> IStructEnumerable<
        Enumerator,
        Entity
    >.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, Entity>(GetEnumerator());
    }

    public void CopyTo(Entity[] array)
    {
        _sparseSet.CopyTo(array);
    }

    public void CopyTo(in Span<Entity> span, int arrayIndex = 0)
    {
        _sparseSet.CopyTo(span, arrayIndex);
    }

    public ValueEnumerable<Enumerator, Entity> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, Entity>(GetEnumerator());
    }

    public struct Enumerator : IStructEnumerator<Entity>, IValueEnumerator<Entity>
    {
        private readonly EntitySparseSet _sparseSet;
        private int _index;

        internal Enumerator(EntitySparseSet sparseSet)
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

public class EntitySparseSet<TValue> : EntitySparseSet<TValue, ValueList<TValue>>
{
    public EntitySparseSet(Scene scene, int sparseChunkSize = DefaultSparseChunkSize)
        : base(scene, [], sparseChunkSize) { }
}

public class EntitySparseSet<TValue, TStorage>
    : ISparseSet<Entity, TValue, TStorage>,
        IDictionary<Entity, TValue>,
        IReadOnlyDictionary<Entity, TValue>,
        IReadOnlyList<KeyValuePair<Entity, TValue>>,
        IStructEnumerable<EntitySparseSet<TValue, TStorage>.Enumerator, KeyValuePair<Entity, TValue>>
    where TStorage : IList<TValue>
{
    public const int DefaultSparseChunkSize = 2048;
    private ValueSparseSet<EntityId, TValue, TStorage> _sparseSet;

    public EntitySparseSet(Scene scene, in TStorage storage, int sparseChunkSize = DefaultSparseChunkSize)
    {
        _sparseSet = new ValueSparseSet<EntityId, TValue, TStorage>(storage, id => id.Index, sparseChunkSize);
        Scene = scene;
    }

    public Scene Scene { get; }

    public KeyEnumerable Keys => new(this);

    void ICollection<KeyValuePair<Entity, TValue>>.Add(KeyValuePair<Entity, TValue> item)
    {
        if (ContainsKey(item.Key))
            throw new ArgumentException("Duplicate key", nameof(item));
        this[item.Key] = item.Value;
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.Contains(KeyValuePair<Entity, TValue> item)
    {
        return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<Entity, TValue>>.CopyTo(KeyValuePair<Entity, TValue>[] array, int arrayIndex)
    {
        if ((uint)arrayIndex > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("The destination array is not large enough.", nameof(array));
        for (var i = 0; i < Count; i++)
            array[arrayIndex + i] = new KeyValuePair<Entity, TValue>(
                new Entity(_sparseSet.Keys[i], Scene),
                _sparseSet.Values[i]
            );
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.Remove(KeyValuePair<Entity, TValue> item)
    {
        return TryGetValue(item.Key, out var value)
            && EqualityComparer<TValue>.Default.Equals(value, item.Value)
            && Remove(item.Key);
    }

    bool ICollection<KeyValuePair<Entity, TValue>>.IsReadOnly => false;

    void IDictionary<Entity, TValue>.Add(Entity key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException("Duplicate key", nameof(key));
        this[key] = value;
    }

    bool IDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    bool IDictionary<Entity, TValue>.Remove(Entity key)
    {
        return Remove(key);
    }

    bool IDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IDictionary<Entity, TValue>.this[Entity key]
    {
        get => this[key];
        set => this[key] = value;
    }

    ICollection<TValue> IDictionary<Entity, TValue>.Values => _sparseSet.Values.AsReadOnly();

    ICollection<Entity> IDictionary<Entity, TValue>.Keys => new KeyEnumerable(this).AsReadOnly();

    IEnumerable<Entity> IReadOnlyDictionary<Entity, TValue>.Keys => new KeyEnumerable(this);

    IEnumerable<TValue> IReadOnlyDictionary<Entity, TValue>.Values => _sparseSet.Values.AsReadOnly();

    bool IReadOnlyDictionary<Entity, TValue>.TryGetValue(Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetValue(key, out value);
    }

    TValue IReadOnlyDictionary<Entity, TValue>.this[Entity key] => this[key];

    bool IReadOnlyDictionary<Entity, TValue>.ContainsKey(Entity key)
    {
        return ContainsKey(key);
    }

    public ISparseSet<TValue, TStorage>.ValueEnumerable Values => _sparseSet.Values;

    public TValue this[in Entity key]
    {
        get => _sparseSet[key.Id];
        set => _sparseSet[key.Id] = value;
    }

    public int Count => _sparseSet.Count;

    public KeyValuePair<Entity, TValue> this[int index]
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

    public bool ContainsKey(in Entity key)
    {
        return _sparseSet.ContainsKey(key.Id);
    }

    public bool TryGetValue(in Entity key, [MaybeNullWhen(false)] out TValue value)
    {
        return _sparseSet.TryGetValue(key.Id, out value);
    }

    public bool Remove(in Entity key)
    {
        return _sparseSet.Remove(key.Id);
    }

    public int GetKeyIndex(in Entity key)
    {
        return _sparseSet.GetKeyIndex(key.Id);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    ValueEnumerable<
        StructEnumerator<Enumerator, KeyValuePair<Entity, TValue>>,
        KeyValuePair<Entity, TValue>
    > IStructEnumerable<Enumerator, KeyValuePair<Entity, TValue>>.AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, KeyValuePair<Entity, TValue>>(GetEnumerator());
    }

    public ValueEnumerable<Enumerator, KeyValuePair<Entity, TValue>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, KeyValuePair<Entity, TValue>>(GetEnumerator());
    }

    public struct Enumerator
        : IStructEnumerator<KeyValuePair<Entity, TValue>>,
            IValueEnumerator<KeyValuePair<Entity, TValue>>
    {
        private readonly EntitySparseSet<TValue, TStorage> _sparseSet;
        private int _index;

        internal Enumerator(EntitySparseSet<TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
            Reset();
        }

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_sparseSet._sparseSet.Keys.Count)
            {
                Current = new KeyValuePair<Entity, TValue>(
                    new Entity(_sparseSet._sparseSet.Keys[_index], _sparseSet.Scene),
                    _sparseSet._sparseSet.Values[_index]
                );
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

    public readonly struct KeyEnumerable : IReadOnlyList<Entity>, IStructEnumerable<KeyEnumerable.Enumerator, Entity>
    {
        private readonly EntitySparseSet<TValue, TStorage> _sparseSet;

        public int Count => _sparseSet.Count;

        public Entity this[int index] => new(_sparseSet._sparseSet.Keys[index], _sparseSet.Scene);

        internal KeyEnumerable(EntitySparseSet<TValue, TStorage> sparseSet)
        {
            _sparseSet = sparseSet;
        }

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
            private readonly EntitySparseSet<TValue, TStorage> _sparseSet;
            private int _index;

            internal Enumerator(EntitySparseSet<TValue, TStorage> sparseSet)
            {
                _sparseSet = sparseSet;
                Reset();
            }

            public bool MoveNext()
            {
                if ((uint)_index < (uint)_sparseSet._sparseSet.Keys.Count)
                {
                    Current = new Entity(_sparseSet._sparseSet.Keys[_index], _sparseSet.Scene);
                    _index++;
                    return true;
                }

                Current = default!;
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

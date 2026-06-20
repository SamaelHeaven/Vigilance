using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using ZLinq;

namespace Vigilance.Drawing;

public sealed unsafe class VertexBuffer<T> : IValueListView<T>, IDisposable
    where T : unmanaged
{
    private ValueList<bool> _dirtyValues;
    private ValueList<T> _values;
    private int _vboCapacity = 0;

    public VertexBuffer()
    {
        Game.ThrowIfNotRunning();
        _values = [];
        _dirtyValues = [];
    }

    public VertexBuffer(int capacity)
    {
        Game.ThrowIfNotRunning();
        _values = new ValueList<T>(capacity);
        _dirtyValues = new ValueList<bool>(capacity);
    }

    public VertexBuffer(in ReadOnlySpan<T> values)
    {
        Game.ThrowIfNotRunning();
        _values = values.AsValueEnumerable().ToValueList();
        _dirtyValues = new ValueList<bool>(_values.Count) { Count = _values.Count };
    }

    public VertexBuffer(in IEnumerable<T> values)
    {
        Game.ThrowIfNotRunning();
        _values = values.AsValueEnumerable().ToValueList();
        _dirtyValues = new ValueList<bool>(_values.Count) { Count = _values.Count };
    }

    public uint Id { get; private set; } = 0;

    public bool IsValid => Id != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        Id = 0;
    }

    public int Count => _values.Count;

    public T this[int index]
    {
        get => _values[index];
        set
        {
            _values[index] = value;
            _dirtyValues[index] = true;
        }
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return _values.AsSpan();
    }

    public ValueList<T>.Enumerator GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    public ValueEnumerable<ValueList<T>.Enumerator, T> AsValueEnumerable()
    {
        return _values.AsValueEnumerable();
    }

    public void Add(in T value)
    {
        _values.Add(value);
        _dirtyValues.Add(true);
    }

    public void Clear()
    {
        _values.Clear();
        _dirtyValues.Clear();
    }

    public void RemoveAt(int index)
    {
        _values.RemoveAt(index);
        _dirtyValues.Count--;
        for (var i = index; i < _dirtyValues.Count; i++)
            _dirtyValues[i] = true;
    }

    public void Sync()
    {
        if (Id != 0 && _values.Capacity > _vboCapacity)
        {
            Rlgl.UnloadVertexBuffer(Id);
            Id = 0;
        }

        if (Id == 0)
        {
            _vboCapacity = _values.Capacity;
            fixed (void* buffer = _values.AsSpan())
            {
                Id = Rlgl.LoadVertexBuffer(buffer, _vboCapacity * sizeof(T), true);
            }

            _dirtyValues.AsSpan().Clear();
            return;
        }

        var span = _values.AsSpan();
        var dirty = _dirtyValues.AsSpan();
        var i = 0;
        var count = span.Length;
        var isDirty = false;
        while (i < count)
        {
            while (i < count && !dirty[i])
                i++;
            if (i >= count)
                break;
            isDirty = true;
            var start = i;
            while (i < count && dirty[i])
                i++;
            var length = i - start;
            fixed (T* ptr = &span[start])
            {
                Rlgl.UpdateVertexBuffer(Id, ptr, length * sizeof(T), start * sizeof(T));
            }
        }

        if (isDirty)
            dirty.Clear();
    }

    private void ReleaseUnmanagedResources()
    {
        if (Id != 0)
            Game.Defer(() => Rlgl.UnloadVertexBuffer(Id));
    }

    ~VertexBuffer()
    {
        ReleaseUnmanagedResources();
    }
}

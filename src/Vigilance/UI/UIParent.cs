#pragma warning disable CS9084

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vigilance.Collections;
using ZLinq;

namespace Vigilance.UI;

public abstract class UIParent : UIElement
{
    private ValueList<UIElement> _childrenList = [];
    private ValueQueue<ChildrenOperation> _childrenOperations = [];
    private int _deferredCount;
    private bool _isFlushing;
    private ValueStack<int> _suspendStack = [];

    public bool IsDeferred => _deferredCount != 0;

    public UIParent this[UIElement? element]
    {
        get
        {
            Add(element);
            return this;
        }
    }

    public UIParent this[params ReadOnlySpan<UIElement?> elements]
    {
        get
        {
            Add(elements);
            return this;
        }
    }

    public UIParent this[IEnumerable<UIElement?> elements]
    {
        get
        {
            foreach (var element in elements)
                Add(element);
            return this;
        }
    }

    public ChildEnumerable Children()
    {
        return new ChildEnumerable(this);
    }

    public void Add(UIElement? element)
    {
        if (element is null)
            return;
        if (IsDeferred)
        {
            _childrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Add, element));
            return;
        }

        element.Remove();
        _childrenList.Add(element);
        element.Parent = this;
        MarkDirty();
    }

    public void Add(params ReadOnlySpan<UIElement?> elements)
    {
        foreach (var element in elements)
            Add(element);
    }

    public void Insert(int index, UIElement element)
    {
        if (IsDeferred)
        {
            _childrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Insert, element, index));
            return;
        }

        _childrenList.Insert(index, element);
        element.Remove();
        element.Parent = this;
        MarkDirty();
    }

    public int IndexOf(UIElement element)
    {
        return _childrenList.IndexOf(element);
    }

    public void Replace(int index, UIElement element)
    {
        if (IsDeferred)
        {
            _childrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Replace, element, index));
            return;
        }

        _childrenList[index].Remove();
        element.Remove();
        element.Parent = this;
        _childrenList[index] = element;
        MarkDirty();
    }

    public void Clear()
    {
        foreach (var element in Children())
            element.Remove();
    }

    public void BeginDefer()
    {
        _deferredCount++;
    }

    public void EndDefer()
    {
        if (_deferredCount == 0)
            throw new InvalidOperationException("Element is not in a deferred state.");
        _deferredCount--;
        TryFlush();
    }

    public void SuspendDefer()
    {
        _suspendStack.Push(_deferredCount);
        _deferredCount = 0;
    }

    public void ResumeDefer()
    {
        if (_suspendStack.Count == 0)
            throw new InvalidOperationException("Element is not in a suspended state.");
        _deferredCount += _suspendStack.Pop();
    }

    internal void Clone(CloneOptions options)
    {
        _childrenList = options.HasFlag(CloneOptions.SkipChildren) ? [] : new ValueList<UIElement>(_childrenList.Count);
        _childrenOperations = [];
        _deferredCount = 0;
        _suspendStack = [];
        _isFlushing = false;
    }

    private void TryFlush()
    {
        if (_deferredCount != 0 || _isFlushing)
            return;
        _isFlushing = true;
        while (_childrenOperations.TryDequeue(out var operation))
            operation.Execute(this);
        _isFlushing = false;
    }

    internal bool Remove(UIElement element)
    {
        if (IsDeferred)
        {
            _childrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Remove, element));
            return false;
        }

        var result = _childrenList.Remove(element);
        element.Parent = null;
        MarkDirty();
        return result;
    }

    internal enum ChildrenOperationType : sbyte
    {
        Add,
        Remove,
        Insert,
        Replace,
    }

    internal readonly record struct ChildrenOperation(
        ChildrenOperationType Type,
        UIElement? Element = null,
        int Index = 0
    )
    {
        public void Execute(UIParent parent)
        {
            switch (Type)
            {
                case ChildrenOperationType.Add:
                    parent.Add(Element);
                    break;
                case ChildrenOperationType.Remove:
                    parent.Remove(Element!);
                    break;
                case ChildrenOperationType.Insert:
                    parent.Insert(Index, Element!);
                    break;
                case ChildrenOperationType.Replace:
                    parent.Replace(Index, Element!);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(ChildrenOperationType));
            }
        }
    }

    public unsafe struct ChildEnumerable : IStructEnumerable<ChildEnumerator, UIElement>, IReadOnlyList<UIElement>
    {
        private readonly UIParent _parent;
        private bool _deferred;

        internal ChildEnumerable(UIParent parent)
        {
            _parent = parent;
            _deferred = true;
        }

        public ChildEnumerator GetEnumerator()
        {
            return new ChildEnumerator(_parent, _deferred);
        }

        public ValueEnumerable<ChildEnumerator, UIElement> AsValueEnumerable()
        {
            return new ValueEnumerable<ChildEnumerator, UIElement>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<ChildEnumerator, UIElement>, UIElement> IStructEnumerable<
            ChildEnumerator,
            UIElement
        >.AsValueEnumerable()
        {
            return new StructEnumerator<ChildEnumerator, UIElement>(GetEnumerator());
        }

        public int Count => _parent._childrenList.Count;

        public UIElement this[int index] => _parent._childrenList[index];

        public ref ChildEnumerable Deferred(bool deferred = true)
        {
            _deferred = deferred;
            return ref this;
        }
    }

    public struct ChildEnumerator : IStructEnumerator<UIElement>, IValueEnumerator<UIElement>
    {
        private readonly UIParent _parent;
        private int _index;
        private readonly bool _deferred;
        private bool _initialized;
        private bool _disposed;

        internal ChildEnumerator(UIParent parent, bool deferred)
        {
            _parent = parent;
            _deferred = deferred;
            _initialized = false;
            _disposed = true;
        }

        private void Initialize()
        {
            _index = 0;
            Current = null!;
            _initialized = true;
            _disposed = false;
            if (_deferred)
                _parent.BeginDefer();
        }

        public bool MoveNext()
        {
            if (!_initialized)
                Initialize();
            if ((uint)_index < (uint)_parent._childrenList.Count)
            {
                Current = _parent._childrenList[_index];
                _index++;
                return true;
            }

            Current = null!;
            _index = -1;
            return false;
        }

        public void Reset()
        {
            Dispose();
            _initialized = false;
        }

        public UIElement Current { get; private set; } = null!;

        public void Dispose()
        {
            if (_disposed)
                return;
            if (_deferred)
                _parent.EndDefer();
            _disposed = true;
        }

        public bool TryGetNext(out UIElement current)
        {
            if (MoveNext())
            {
                current = Current;
                return true;
            }

            Unsafe.SkipInit(out current);
            return false;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _parent._childrenList.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<UIElement> span)
        {
            span = _parent._childrenList.AsSpan();
            return true;
        }

        public bool TryCopyTo(scoped Span<UIElement> destination, Index offset)
        {
            return _parent._childrenList.AsSpan().TryCopyTo(destination, offset);
        }
    }
}

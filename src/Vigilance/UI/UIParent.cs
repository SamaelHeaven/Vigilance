#pragma warning disable CS9084

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vigilance.Collections;
using ZLinq;
using ZLinq.Internal;

namespace Vigilance.UI;

public abstract class UIParent : UIElement
{
    internal ValueList<UIElement> ChildrenList = [];
    internal ValueQueue<ChildrenOperation> ChildrenOperations = [];
    public bool IsDeferred => DeferredCount != 0 && SuspendedCount == 0;
    public int DeferredCount { get; internal set; }
    public int SuspendedCount { get; private set; }

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
            AddRange(elements);
            return this;
        }
    }

    public ChildEnumerable Children => new(this);

    public void Add(UIElement? element)
    {
        if (element is null)
            return;
        if (IsDeferred)
        {
            ChildrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Add, element));
            return;
        }

        element.Remove();
        ChildrenList.Add(element);
        element.Parent = this;
        if (!IsLayoutCustom)
            Node.AddChild(element.Node);
        MarkDirty();
    }

    public void Add(params ReadOnlySpan<UIElement?> elements)
    {
        foreach (var element in elements)
            Add(element);
    }

    public void AddRange<T>(T elements)
        where T : IEnumerable<UIElement?>
    {
        foreach (var element in elements)
            Add(element);
    }

    public void Insert(int index, UIElement element)
    {
        if (IsDeferred)
        {
            ChildrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Insert, element, index));
            return;
        }

        ChildrenList.Insert(index, element);
        element.Remove();
        element.Parent = this;
        if (!IsLayoutCustom)
            Node.InsertChild(element.Node, index);
        MarkDirty();
    }

    public int IndexOf(UIElement element)
    {
        return ChildrenList.IndexOf(element);
    }

    public void Replace(int index, UIElement element)
    {
        if (IsDeferred)
        {
            ChildrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Replace, element, index));
            return;
        }

        ChildrenList[index].Remove();
        element.Remove();
        element.Parent = this;
        ChildrenList[index] = element;
        if (!IsLayoutCustom)
            Node.ReplaceChild(index, element.Node);
        MarkDirty();
    }

    public void Clear()
    {
        foreach (var element in Children)
            element.Remove();
    }

    public void BeginDefer()
    {
        DeferredCount++;
    }

    public void EndDefer()
    {
        if (DeferredCount == 0)
            throw new InvalidOperationException("Element is not in a deferred state.");
        if (--DeferredCount != 0)
            return;
        while (ChildrenOperations.TryDequeue(out var operation))
            operation.Execute(this);
    }

    public void SuspendDefer()
    {
        SuspendedCount++;
    }

    public void ResumeDefer()
    {
        if (SuspendedCount == 0)
            throw new InvalidOperationException("Element is not in a suspended state.");
        SuspendedCount--;
    }

    internal void Remove(UIElement element)
    {
        if (IsDeferred)
        {
            ChildrenOperations.Enqueue(new ChildrenOperation(ChildrenOperationType.Remove, element));
            return;
        }

        ChildrenList.Remove(element);
        element.Parent = null;
        if (!IsLayoutCustom)
            Node.RemoveChild(element.Node);
        MarkDirty();
    }

    internal enum ChildrenOperationType
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

        public int Count => _parent.ChildrenList.Count;

        public UIElement this[int index] => _parent.ChildrenList[index];

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
        private bool _disposed;

        internal ChildEnumerator(UIParent parent, bool deferred)
        {
            _parent = parent;
            _deferred = deferred;
            Reset();
        }

        public bool MoveNext()
        {
            var newIndex = _index + 1;
            if (newIndex >= _parent.ChildrenList.Count)
                return false;
            _index = newIndex;
            return true;
        }

        public void Reset()
        {
            Dispose();
            _parent.BeginDefer();
            _index = -1;
            _disposed = false;
        }

        public UIElement Current => _parent.ChildrenList[_index];

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
            count = _parent.ChildrenList.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<UIElement> span)
        {
            span = _parent.ChildrenList.AsSpan();
            return true;
        }

        public bool TryCopyTo(scoped Span<UIElement> destination, Index offset)
        {
            if (!EnumeratorHelper.TryGetSlice(_parent.ChildrenList.AsSpan(), offset, destination.Length, out var slice))
                return false;
            slice.CopyTo(destination);
            return true;
        }
    }
}

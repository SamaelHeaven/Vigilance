#pragma warning disable CS9084

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using LinkDotNet.StringBuilder;
using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public readonly unsafe partial record struct Entity : IComparable<Entity>
{
    internal Entity(ulong id, Scene scene)
    {
        Index = GetIndex(id);
        Generation = GetGeneration(id);
        Scene = scene;
    }

    internal Entity(int index, int generation, Scene scene)
    {
        Index = index;
        Generation = generation;
        Scene = scene;
    }

    public int Index { get; }
    public int Generation { get; }
    public Scene Scene { get; }

    public static Entity Null => default;

    public bool IsNull => Index == 0;

    public bool IsValid => !Scene.Lookup(Index, Generation).IsNull;

    public ulong Id => GetId(Index, Generation);

    public string Path => this.AncestorsAndSelf().Select(e => e.Name).Reverse().JoinToString(".");

    public string Name
    {
        get
        {
            EnsureValid();
            return Get<Name>();
        }
    }

    public Entity Parent
    {
        get
        {
            EnsureValid();
            var child = GetRef<Child>();
            return child.IsNull ? Null : new Entity(child.Read.ParentId, Scene);
        }
        set
        {
            EnsureValid();
            value = Scene.Lookup(value.Index, value.Generation);
            if (value.IsNull)
                Remove<Child>();
            else
                Set(new Child(value.Id));
        }
    }

    public bool IsDisabled
    {
        get
        {
            EnsureValid();
            return Has<Disabled>();
        }
        set
        {
            EnsureValid();
            if (value)
                Set<Disabled>();
            else
                Remove<Disabled>();
        }
    }

    public Transform WorldTransform
    {
        get
        {
            EnsureValid();
            var transform = Transform;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                transform += entity.Transform;
            return transform;
        }
    }

    public Vector2 WorldPosition
    {
        get
        {
            EnsureValid();
            var position = Position;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                position += entity.Position;
            return position;
        }
    }

    public Vector2 WorldScale
    {
        get
        {
            EnsureValid();
            var scale = Scale;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                scale *= entity.Scale;
            return scale;
        }
    }

    public float WorldRotation
    {
        get
        {
            EnsureValid();
            var rotation = Rotation;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                rotation += entity.Rotation;
            return rotation;
        }
    }

    public Vector2 WorldPivotPoint
    {
        get
        {
            EnsureValid();
            var pivotPoint = PivotPoint;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                pivotPoint += entity.PivotPoint;
            return pivotPoint;
        }
    }

    public int WorldZIndex
    {
        get
        {
            EnsureValid();
            var zIndex = ZIndex;
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
                zIndex += entity.ZIndex;
            return zIndex;
        }
    }

    public Transform Transform
    {
        get
        {
            EnsureValid();
            return Get<Transform>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<Transform>();
            ref var transform = ref table.GetRef(this);
            var oldTransform = transform;
            if (Precision.AreEqual(value, oldTransform))
                return;
            transform = value;
            var positionTable = Scene.Table<Position>();
            ref var position = ref positionTable.GetRef(this);
            var oldPosition = position;
            var positionChanged = !Precision.AreEqual(value.Position, oldPosition);
            if (positionChanged)
                position.Value = value.Position;
            var scaleTable = Scene.Table<Scale>();
            ref var scale = ref scaleTable.GetRef(this);
            var oldScale = scale;
            var scaleChanged = !Precision.AreEqual(value.Scale, oldScale);
            if (scaleChanged)
                scale.Value = value.Scale;
            var rotationTable = Scene.Table<Rotation>();
            ref var rotation = ref rotationTable.GetRef(this);
            var oldRotation = rotation;
            var rotationChanged = !Precision.AreEqual(value.Rotation, oldRotation);
            if (rotationChanged)
                rotation.Value = value.Rotation;
            var pivotPointTable = Scene.Table<PivotPoint>();
            ref var pivotPoint = ref pivotPointTable.GetRef(this);
            var oldPivotPoint = pivotPoint;
            var pivotPointChanged = !Precision.AreEqual(value.PivotPoint, oldPivotPoint);
            if (pivotPointChanged)
                pivotPoint.Value = value.PivotPoint;
            table.Enqueue(Table.Event<Transform>.Set(this, oldTransform, value));
            if (positionChanged)
                positionTable.Enqueue(Table.Event<Position>.Set(this, oldPosition, value.Position));
            if (scaleChanged)
                scaleTable.Enqueue(Table.Event<Scale>.Set(this, oldScale, value.Scale));
            if (rotationChanged)
                rotationTable.Enqueue(Table.Event<Rotation>.Set(this, oldRotation, value.Rotation));
            if (pivotPointChanged)
                pivotPointTable.Enqueue(Table.Event<PivotPoint>.Set(this, oldPivotPoint, value.PivotPoint));
        }
    }

    public Vector2 Position
    {
        get
        {
            EnsureValid();
            return Get<Position>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<Position>();
            ref var position = ref table.GetRef(this);
            var oldPosition = position;
            if (Precision.AreEqual(value, oldPosition))
                return;
            position.Value = value;
            var transformTable = Scene.Table<Transform>();
            ref var transform = ref transformTable.GetRef(this);
            var oldTransform = transform;
            transform.Position = value;
            table.Enqueue(Table.Event<Position>.Set(this, oldPosition, value));
            transformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public Vector2 Scale
    {
        get
        {
            EnsureValid();
            return Get<Scale>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<Scale>();
            ref var scale = ref table.GetRef(this);
            var oldScale = scale;
            if (Precision.AreEqual(value, oldScale))
                return;
            scale.Value = value;
            var transformTable = Scene.Table<Transform>();
            ref var transform = ref transformTable.GetRef(this);
            var oldTransform = transform;
            transform.Scale = value;
            table.Enqueue(Table.Event<Scale>.Set(this, oldScale, value));
            transformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public float Rotation
    {
        get
        {
            EnsureValid();
            return Get<Rotation>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<Rotation>();
            ref var rotation = ref table.GetRef(this);
            var oldRotation = rotation;
            if (Precision.AreEqual(value, oldRotation))
                return;
            rotation.Value = value;
            var transformTable = Scene.Table<Transform>();
            ref var transform = ref transformTable.GetRef(this);
            var oldTransform = transform;
            transform.Rotation = value;
            table.Enqueue(Table.Event<Rotation>.Set(this, oldRotation, value));
            transformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public Vector2 PivotPoint
    {
        get
        {
            EnsureValid();
            return Get<PivotPoint>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<PivotPoint>();
            ref var pivotPoint = ref table.GetRef(this);
            var oldPivotPoint = pivotPoint;
            if (Precision.AreEqual(value, oldPivotPoint))
                return;
            pivotPoint.Value = value;
            var transformTable = Scene.Table<Transform>();
            ref var transform = ref transformTable.GetRef(this);
            var oldTransform = transform;
            transform.PivotPoint = value;
            table.Enqueue(Table.Event<PivotPoint>.Set(this, oldPivotPoint, value));
            transformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public int ZIndex
    {
        get
        {
            EnsureValid();
            return Get<ZIndex>();
        }
        set
        {
            EnsureValid();
            var table = Scene.Table<ZIndex>();
            ref var zIndex = ref table.GetRef(this);
            var oldZIndex = zIndex;
            if (value == oldZIndex.Value)
                return;
            zIndex.Value = value;
            table.Enqueue(Table.Event<ZIndex>.Set(this, oldZIndex, value));
        }
    }

    public ulong Order
    {
        get
        {
            EnsureValid();
            return ((ulong)(uint)(WorldZIndex ^ int.MinValue) << 32) | (uint)Index;
        }
    }

    public TableEnumerable Tables => new(this);

    public ComponentEnumerable Components => new(this);

    public ChildEnumerable Children => new(this);

    public int CompareTo(Entity other)
    {
        EnsureValid();
        other.EnsureValid();
        return Order.CompareTo(other.Order);
    }

    public static ulong GetId(int index, int generation)
    {
        return ((ulong)(uint)generation << 32) | (uint)index;
    }

    public static int GetIndex(ulong id)
    {
        return (int)(id & 0xFFFFFFFF);
    }

    public static int GetGeneration(ulong id)
    {
        return (int)(id >> 32);
    }

    public T Get<T>()
    {
        EnsureValid();
        return Scene.Table<T>().GetRef(this);
    }

    public object? Get(Table table)
    {
        EnsureValid();
        return table.Get(this);
    }

    public bool TryGet<T>(out T value)
    {
        EnsureValid();
        Unsafe.SkipInit(out value);
        var data = new ComponentRef<T>(ref Scene.Table<T>().GetRef(this));
        if (data.IsNull)
            return false;
        value = data;
        return true;
    }

    public bool TryGet(Table table, out object value)
    {
        EnsureValid();
        value = null!;
        var data = table.Get(this);
        if (data is null)
            return false;
        value = data;
        return true;
    }

    public T GetOrDefault<T>(in T defaultValue)
    {
        EnsureValid();
        var value = new ComponentRef<T>(ref Scene.Table<T>().GetRef(this));
        return value.IsNull ? defaultValue : value;
    }

    public T GetOrDefault<T>(Func<T> defaultFunc)
    {
        EnsureValid();
        var value = new ComponentRef<T>(ref Scene.Table<T>().GetRef(this));
        return value.IsNull ? defaultFunc.Invoke() : value;
    }

    public object? GetOrDefault(Table table, object? defaultValue)
    {
        EnsureValid();
        return table.Get(this) ?? defaultValue;
    }

    public object? GetOrDefault(Table table, Func<object?> defaultValue)
    {
        EnsureValid();
        return table.Get(this) ?? defaultValue.Invoke();
    }

    public ComponentRef<T> GetRef<T>()
    {
        EnsureValid();
        return new ComponentRef<T>(ref Scene.Table<T>().GetRef(this));
    }

    [OverloadResolutionPriority(1)]
    public ref readonly Entity Set<T>(IComposable<T> composable)
    {
        EnsureValid();
        Set(composable.ToComponent());
        return ref this;
    }

    [OverloadResolutionPriority(1)]
    public ref readonly Entity Set<T>(IComposable<T> composable, out ComponentRef<T> componentRef)
    {
        EnsureValid();
        Set(composable.ToComponent(), out componentRef);
        return ref this;
    }

    public ref readonly Entity Set<T>()
        where T : new()
    {
        EnsureValid();
        Scene.Table<T>().Set(this, new T());
        return ref this;
    }

    public ref readonly Entity Set<T>(out ComponentRef<T> componentRef)
        where T : new()
    {
        EnsureValid();
        var value = new T();
        ref var reference = ref Scene.Table<T>().Set(this, value);
#pragma warning disable CS9082
        componentRef = new ComponentRef<T>(ref reference);
#pragma warning restore CS9082
        return ref this;
    }

    public ref readonly Entity Set<T>(in T value)
    {
        EnsureValid();
        Scene.Table<T>().Set(this, value);
        return ref this;
    }

    public ref readonly Entity Set<T>(T value, out ComponentRef<T> componentRef)
    {
        EnsureValid();
#pragma warning disable CS9087
        componentRef = new ComponentRef<T>(ref Scene.Table<T>().Set(this, value));
#pragma warning restore CS9087
        return ref this;
    }

    public ref readonly Entity Set(Table table, object? value)
    {
        EnsureValid();
        table.Set(this, value);
        return ref this;
    }

    public ref readonly Entity Remove<T>(bool ignoreErrors = false)
    {
        EnsureValid();
        Scene
            .Table<T>()
            .Remove(this, ignoreErrors ? Table.OperationStrategy.IgnoreErrors : Table.OperationStrategy.Default);
        return ref this;
    }

    public ref readonly Entity Remove(Table table, bool ignoreErrors = false)
    {
        EnsureValid();
        table.Remove(this, ignoreErrors ? Table.OperationStrategy.IgnoreErrors : Table.OperationStrategy.Default);
        return ref this;
    }

    public void Clear()
    {
        EnsureValid();
        foreach (var table in Tables.WithHidden())
            Remove(table, true);
    }

    public void Destroy()
    {
        EnsureValid();
        Scene.Destroy(this);
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureValid()
    {
        Debug.Assert(IsValid, "Entity must be valid.");
    }

    public ref readonly Entity SetTransform(in Transform transform)
    {
        EnsureValid();
        Transform = transform;
        return ref this;
    }

    public ref readonly Entity SetPosition(float v1, float? v2 = null)
    {
        EnsureValid();
        Position = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetPosition(Vector2 position)
    {
        EnsureValid();
        Position = position;
        return ref this;
    }

    public ref readonly Entity SetScale(float v1, float? v2 = null)
    {
        EnsureValid();
        Scale = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetScale(Vector2 scale)
    {
        EnsureValid();
        Scale = scale;
        return ref this;
    }

    public ref readonly Entity SetRotation(float rotation)
    {
        EnsureValid();
        Rotation = rotation;
        return ref this;
    }

    public ref readonly Entity SetPivotPoint(float v1, float? v2 = null)
    {
        EnsureValid();
        PivotPoint = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    public ref readonly Entity SetPivotPoint(Vector2 pivotPoint)
    {
        EnsureValid();
        PivotPoint = pivotPoint;
        return ref this;
    }

    public ref readonly Entity SetZIndex(int zIndex)
    {
        EnsureValid();
        ZIndex = zIndex;
        return ref this;
    }

    public ref readonly Entity SetDisabled(bool disabled = true)
    {
        EnsureValid();
        IsDisabled = disabled;
        return ref this;
    }

    public ref readonly Entity SetParent(in Entity parent)
    {
        EnsureValid();
        Parent = parent;
        return ref this;
    }

    public ref readonly Entity Scope(Action action)
    {
        EnsureValid();
        var previousScope = Scene.SetScope(this);
        try
        {
            action.Invoke();
        }
        finally
        {
            Scene.SetScope(previousScope);
        }

        return ref this;
    }

    public ref readonly Entity Scope(Action<Scene> action)
    {
        EnsureValid();
        var previousScope = Scene.SetScope(this);
        try
        {
            action.Invoke(Scene);
        }
        finally
        {
            Scene.SetScope(previousScope);
        }

        return ref this;
    }

    private bool PrintMembers(StringBuilder sb)
    {
        if (Id == 0)
        {
            sb.Append("Null");
            return true;
        }

        sb.Append("Id = ");
        sb.Append(Id);
        if (!IsValid)
        {
            sb.Append(", Valid = ");
            sb.Append(IsValid);
            return true;
        }

        var path = Path;
        if (path != Name)
        {
            sb.Append(", Path = ");
            sb.Append(Path);
        }

        sb.Append(", Components = ");
        sb.Append(Components.ToString());
        return true;
    }

    public struct TableEnumerable : IStructEnumerable<TableEnumerator, Table>
    {
        private readonly Entity _entity;
        private bool _withHidden;

        internal TableEnumerable(in Entity entity)
        {
            _entity = entity;
        }

        public TableEnumerator GetEnumerator()
        {
            return new TableEnumerator(_entity, _withHidden);
        }

        public ValueEnumerable<StructEnumerator<TableEnumerator, Table>, Table> AsValueEnumerable()
        {
            return new StructEnumerator<TableEnumerator, Table>(GetEnumerator());
        }

        public ref TableEnumerable WithHidden(bool withHidden = true)
        {
            _withHidden = withHidden;
            return ref this;
        }
    }

    public struct TableEnumerator : IStructEnumerator<Table>
    {
        private readonly Entity _entity;
        private readonly bool _withHidden;
        private Scene.TableEnumerator _enumerator;

        internal TableEnumerator(in Entity entity, bool withHidden)
        {
            _entity = entity;
            _withHidden = withHidden;
            Reset();
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var table = _enumerator.Current;
                if (!_entity.Has(table))
                    continue;
                Current = table;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _entity.EnsureValid();
            _enumerator = _entity.Scene.Tables.WithHidden(_withHidden).GetEnumerator();
            Current = null!;
        }

        public Table Current { get; private set; } = null!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public struct ComponentEnumerable : IStructEnumerable<ComponentEnumerator, object>
    {
        private readonly Entity _entity;
        private bool _withHidden;

        internal ComponentEnumerable(in Entity entity)
        {
            _entity = entity;
        }

        public ComponentEnumerator GetEnumerator()
        {
            return new ComponentEnumerator(_entity, _withHidden);
        }

        public ValueEnumerable<StructEnumerator<ComponentEnumerator, object>, object> AsValueEnumerable()
        {
            return new StructEnumerator<ComponentEnumerator, object>(GetEnumerator());
        }

        public ref ComponentEnumerable WithHidden(bool withHidden = true)
        {
            _withHidden = withHidden;
            return ref this;
        }

        public override string ToString()
        {
            using var sb = new ValueStringBuilder(stackalloc char[256]);
            sb.Append('[');
            var any = false;
            foreach (var component in this)
            {
                any = true;
                sb.Append($"\n {component}, ");
            }

            if (any)
                sb.Append('\n');
            sb.Append(']');
            return sb.ToString();
        }
    }

    public struct ComponentEnumerator : IStructEnumerator<object>
    {
        private readonly Entity _entity;
        private readonly bool _withHidden;
        private Scene.TableEnumerator _enumerator;

        internal ComponentEnumerator(in Entity entity, bool withHidden)
        {
            _entity = entity;
            _withHidden = withHidden;
            Reset();
        }

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var table = _enumerator.Current;
                if (!_entity.TryGet(table, out var value))
                    continue;
                Current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _entity.EnsureValid();
            _enumerator = _entity.Scene.Tables.WithHidden(_withHidden).GetEnumerator();
            Current = null!;
        }

        public object Current { get; private set; } = null!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public struct ChildEnumerable : IStructEnumerable<ChildEnumerator, Entity>
    {
        private readonly Entity _parent;
        private bool _deferred;

        internal ChildEnumerable(in Entity parent)
        {
            _parent = parent;
            _deferred = true;
        }

        public ChildEnumerator GetEnumerator()
        {
            return new ChildEnumerator(_parent, _deferred);
        }

        public ValueEnumerable<StructEnumerator<ChildEnumerator, Entity>, Entity> AsValueEnumerable()
        {
            return new StructEnumerator<ChildEnumerator, Entity>(GetEnumerator());
        }

        public ref ChildEnumerable Deferred(bool deferred = true)
        {
            _deferred = deferred;
            return ref this;
        }
    }

    public struct ChildEnumerator : IStructEnumerator<Entity>
    {
        private readonly Entity _parent;
        private readonly bool _deferred;
        private ulong _nextChildId;
        private bool _disposed;

        internal ChildEnumerator(in Entity parent, bool deferred)
        {
            _parent = parent;
            _deferred = deferred;
            Reset();
        }

        public bool MoveNext()
        {
            if (_nextChildId == 0)
            {
                Current = default;
                return false;
            }

            Current = new Entity(_nextChildId, _parent.Scene);
            var childRef = Current.GetRef<Child>();
            _nextChildId = childRef.IsNull ? 0 : childRef.Read.NextSiblingId;
            return true;
        }

        public void Reset()
        {
            if (_nextChildId > 0)
                Dispose();
            _parent.EnsureValid();
            var parentRef = _parent.GetRef<Parent>();
            _nextChildId = parentRef.IsNull ? 0 : parentRef.Read.FirstChildId;
            Current = Null;
            _disposed = false;
            if (_deferred)
                _parent.Scene.BeginDefer();
        }

        public Entity Current { get; private set; }

        public void Dispose()
        {
            if (_disposed)
                return;
            if (_deferred)
                _parent.Scene.EndDefer();
            _disposed = true;
        }
    }

    public struct Traverser : ITraverser<Traverser, Entity>
    {
        private ulong _nextChildId;
        private ulong _nextSiblingId;
        private ulong _previousSiblingId;
        private readonly bool _deferred;
        private bool _hasDeferBegun;
        private bool _childrenInitialized;
        private bool _nextSiblingInitialized;
        private bool _previousSiblingInitialized;

        public Entity Origin { get; }

        internal Traverser(in Entity origin, bool deferred = true)
        {
            origin.EnsureValid();
            Origin = origin;
            _deferred = deferred;
        }

        private void EnsureDeferred()
        {
            if (!_deferred || _hasDeferBegun)
                return;
            Origin.Scene.BeginDefer();
            _hasDeferBegun = true;
        }

        public Traverser ConvertToTraverser(Entity next)
        {
            return new Traverser(next, _deferred);
        }

        public bool TryGetChildCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetHasChild(out bool hasChild)
        {
            EnsureDeferred();
            var parentRef = Origin.GetRef<Parent>();
            hasChild = !parentRef.IsNull && parentRef.Read.FirstChildId != 0;
            return true;
        }

        public bool TryGetParent(out Entity parent)
        {
            EnsureDeferred();
            parent = Origin.Parent;
            return !parent.IsNull;
        }

        public bool TryGetNextChild(out Entity child)
        {
            EnsureDeferred();
            if (!_childrenInitialized)
            {
                var parentRef = Origin.GetRef<Parent>();
                _nextChildId = parentRef.IsNull ? 0 : parentRef.Read.FirstChildId;
                _childrenInitialized = true;
            }

            if (_nextChildId == 0)
            {
                child = Null;
                return false;
            }

            child = new Entity(_nextChildId, Origin.Scene);
            var childRef = child.GetRef<Child>();
            _nextChildId = childRef.IsNull ? 0 : childRef.Read.NextSiblingId;
            return true;
        }

        public bool TryGetNextSibling(out Entity next)
        {
            EnsureDeferred();
            if (!_nextSiblingInitialized)
            {
                var childRef = Origin.GetRef<Child>();
                _nextSiblingId = childRef.IsNull ? 0 : childRef.Read.NextSiblingId;
                _nextSiblingInitialized = true;
            }

            if (_nextSiblingId == 0)
            {
                next = Null;
                return false;
            }

            next = new Entity(_nextSiblingId, Origin.Scene);
            var nextChildRef = next.GetRef<Child>();
            _nextSiblingId = nextChildRef.IsNull ? 0 : nextChildRef.Read.NextSiblingId;
            return true;
        }

        public bool TryGetPreviousSibling(out Entity previous)
        {
            EnsureDeferred();
            if (!_previousSiblingInitialized)
            {
                var childRef = Origin.GetRef<Child>();
                _previousSiblingId = childRef.IsNull ? 0 : childRef.Read.PreviousSiblingId;
                _previousSiblingInitialized = true;
            }

            if (_previousSiblingId == 0)
            {
                previous = Null;
                return false;
            }

            previous = new Entity(_previousSiblingId, Origin.Scene);
            var prevChildRef = previous.GetRef<Child>();
            _previousSiblingId = prevChildRef.IsNull ? 0 : prevChildRef.Read.PreviousSiblingId;
            return true;
        }

        public void Dispose()
        {
            if (!_hasDeferBegun)
                return;
            Origin.Scene.EndDefer();
            _hasDeferBegun = false;
        }
    }
}

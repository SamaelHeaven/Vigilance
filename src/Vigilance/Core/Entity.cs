using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using LinkDotNet.StringBuilder;
using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Core;

public readonly partial record struct Entity
{
    public Entity(EntityId id, Scene scene)
    {
        Index = id.Index;
        Version = id.Version;
        Scene = scene;
    }

    public Entity(int index, int version, Scene scene)
    {
        Index = index;
        Version = version;
        Scene = scene;
    }

    public int Index { get; }
    public int Version { get; }
    public Scene Scene { get; }

    public static Entity Null => default;

    public bool IsNull => Index == 0;

    public bool IsValid => Scene is not null && Scene.IsValid(this);

    public EntityId Id => new(Index, Version);

    public string Path => this.AncestorsAndSelf().Select(e => e.Name).Reverse().JoinToString(".");

    public string Name
    {
        get
        {
            AssertValid();
            var name = Scene is null ? ComponentRef<Name>.Null : Scene.NameTable.GetRef(this);
            return name.IsNull ? $"#{Id}" : name.Read;
        }
    }

    public Entity Parent
    {
        get
        {
            AssertValid();
            var child = Scene is null ? ComponentRef<Child>.Null : Scene.ChildTable.GetRef(this);
            return child.IsNull ? Null : new Entity(child.Read.ParentId, Scene!);
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            if (value.IsNull)
                Scene.ChildTable.Remove(this);
            else
                Scene.ChildTable.Set(this, new Child(value.Id));
        }
    }

    public bool IsDisabled
    {
        get
        {
            AssertValid();
            return Scene is not null && Scene.DisabledTable.Has(this);
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            if (value)
                Scene.DisabledTable.Set(this, new Disabled());
            else
                Scene.DisabledTable.Remove(this);
        }
    }

    public bool IsParent
    {
        get
        {
            AssertValid();
            return Scene is not null && Scene.ParentTable.Has(this);
        }
    }

    public Vector2 RenderPosition
    {
        get
        {
            AssertValid();
            if (Scene is null)
                return default;
            if (!Scene.InterpolationTable.TryGet(this, out var interpolation))
                interpolation = new Interpolation();
            (Vector2? Start, Vector2 End) position = (interpolation.Start?.Position, interpolation.End.Position);
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
            {
                if (!Scene.InterpolationTable.TryGet(entity, out var childInterpolation))
                    continue;
                position = (
                    position.Start.HasValue || childInterpolation.Start.HasValue
                        ? (position.Start ?? position.End)
                            + (childInterpolation.Start ?? childInterpolation.End).Position
                        : null,
                    position.End + childInterpolation.End.Position
                );
            }

            return !position.Start.HasValue
                ? position.End
                : Vector2.Lerp(
                    position.Start.Value,
                    position.End,
                    Time.FixedAccumulatorSeconds / Time.FixedDeltaSeconds
                );
        }
    }

    public Vector2 RenderScale
    {
        get
        {
            AssertValid();
            if (Scene is null)
                return default;
            if (!Scene.InterpolationTable.TryGet(this, out var interpolation))
                interpolation = new Interpolation();
            (Vector2? Start, Vector2 End) scale = (interpolation.Start?.Scale, interpolation.End.Scale);
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
            {
                if (!Scene.InterpolationTable.TryGet(entity, out var childInterpolation))
                    continue;
                scale = (
                    scale.Start.HasValue || childInterpolation.Start.HasValue
                        ? (scale.Start ?? scale.End) * (childInterpolation.Start ?? childInterpolation.End).Scale
                        : null,
                    scale.End * childInterpolation.End.Scale
                );
            }

            return !scale.Start.HasValue
                ? scale.End
                : Vector2.Lerp(scale.Start.Value, scale.End, Time.FixedAccumulatorSeconds / Time.FixedDeltaSeconds);
        }
    }

    public float RenderRotation
    {
        get
        {
            AssertValid();
            if (Scene is null)
                return 0;
            if (!Scene.InterpolationTable.TryGet(this, out var interpolation))
                interpolation = new Interpolation();
            (float? Start, float End) rotation = (interpolation.Start?.Rotation, interpolation.End.Rotation);
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
            {
                if (!Scene.InterpolationTable.TryGet(entity, out var childInterpolation))
                    continue;
                rotation = (
                    rotation.Start.HasValue || childInterpolation.Start.HasValue
                        ? (rotation.Start ?? rotation.End)
                            + (childInterpolation.Start ?? childInterpolation.End).Rotation
                        : null,
                    rotation.End + childInterpolation.End.Rotation
                );
            }

            return !rotation.Start.HasValue
                ? rotation.End
                : float.LerpAngle(
                    rotation.Start.Value,
                    rotation.End,
                    Time.FixedAccumulatorSeconds / Time.FixedDeltaSeconds
                );
        }
    }

    public Vector2 RenderPivotPoint
    {
        get
        {
            AssertValid();
            if (Scene is null)
                return default;
            if (!Scene.InterpolationTable.TryGet(this, out var interpolation))
                interpolation = new Interpolation();
            (Vector2? Start, Vector2 End) pivotPoint = (interpolation.Start?.PivotPoint, interpolation.End.PivotPoint);
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
            {
                if (!Scene.InterpolationTable.TryGet(entity, out var childInterpolation))
                    continue;
                pivotPoint = (
                    pivotPoint.Start.HasValue || childInterpolation.Start.HasValue
                        ? (pivotPoint.Start ?? pivotPoint.End)
                            + (childInterpolation.Start ?? childInterpolation.End).PivotPoint
                        : null,
                    pivotPoint.End + childInterpolation.End.PivotPoint
                );
            }

            return !pivotPoint.Start.HasValue
                ? pivotPoint.End
                : Vector2.Lerp(
                    pivotPoint.Start.Value,
                    pivotPoint.End,
                    Time.FixedAccumulatorSeconds / Time.FixedDeltaSeconds
                );
        }
    }

    public Transform RenderTransform
    {
        get
        {
            AssertValid();
            if (Scene is null)
                return default;
            if (!Scene.InterpolationTable.TryGet(this, out var interpolation))
                interpolation = new Interpolation();
            for (var entity = Parent; !entity.IsNull; entity = entity.Parent)
            {
                if (!Scene.InterpolationTable.TryGet(entity, out var childInterpolation))
                    continue;
                interpolation = new Interpolation(
                    interpolation.Start.HasValue || childInterpolation.Start.HasValue
                        ? (interpolation.Start ?? interpolation.End)
                            + (childInterpolation.Start ?? childInterpolation.End)
                        : null,
                    interpolation.End + childInterpolation.End
                );
            }

            return !interpolation.Start.HasValue
                ? interpolation.End
                : Transform.Lerp(
                    interpolation.Start.Value,
                    interpolation.End,
                    Time.FixedAccumulatorSeconds / Time.FixedDeltaSeconds
                );
        }
    }

    public Transform WorldTransform
    {
        get
        {
            AssertValid();
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
            AssertValid();
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
            AssertValid();
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
            AssertValid();
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
            AssertValid();
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
            AssertValid();
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
            AssertValid();
            var transform = Scene is null ? ComponentRef<Transform>.Null : Scene.TransformTable.GetRef(this);
            return transform.IsNull ? new Transform() : transform;
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var transform = ref Scene.TransformTable.GetRef(this).Value;
            Transform oldTransform;
            if (Unsafe.IsNullRef(ref transform))
            {
                oldTransform = new Transform();
                Scene.SuspendDefer();
                transform = ref Scene.TransformTable.Set(this, value).Value;
                Scene.ResumeDefer();
            }
            else
            {
                oldTransform = transform;
                if (Precision.AreEqual(value, oldTransform))
                    return;
                transform = value;
            }

            ref var position = ref Scene.PositionTable.GetRef(this).Value;
            var positionNull = Unsafe.IsNullRef(ref position);
            var oldPosition = positionNull ? default : position;
            var positionChanged = positionNull || !Precision.AreEqual(value.Position, oldPosition);
            if (positionChanged)
            {
                if (positionNull)
                {
                    Scene.SuspendDefer();
                    position = ref Scene.PositionTable.Set(this, value.Position).Value;
                    Scene.ResumeDefer();
                }
                else
                {
                    position.Value = value.Position;
                }
            }

            ref var scale = ref Scene.ScaleTable.GetRef(this).Value;
            var scaleNull = Unsafe.IsNullRef(ref scale);
            var oldScale = scaleNull ? new Scale() : scale;
            var scaleChanged = scaleNull || !Precision.AreEqual(value.Scale, oldScale);
            if (scaleChanged)
            {
                if (scaleNull)
                {
                    Scene.SuspendDefer();
                    scale = ref Scene.ScaleTable.Set(this, value.Scale).Value;
                    Scene.ResumeDefer();
                }
                else
                {
                    scale.Value = value.Scale;
                }
            }

            ref var rotation = ref Scene.RotationTable.GetRef(this).Value;
            var rotationNull = Unsafe.IsNullRef(ref rotation);
            var oldRotation = rotationNull ? default : rotation;
            var rotationChanged = rotationNull || !Precision.AreEqual(value.Rotation, oldRotation);
            if (rotationChanged)
            {
                if (rotationNull)
                {
                    Scene.SuspendDefer();
                    rotation = ref Scene.RotationTable.Set(this, value.Rotation).Value;
                    Scene.ResumeDefer();
                }
                else
                {
                    rotation.Value = value.Rotation;
                }
            }

            ref var pivotPoint = ref Scene.PivotPointTable.GetRef(this).Value;
            var pivotPointNull = Unsafe.IsNullRef(ref pivotPoint);
            var oldPivotPoint = pivotPointNull ? default : pivotPoint;
            var pivotPointChanged = pivotPointNull || !Precision.AreEqual(value.PivotPoint, oldPivotPoint);
            if (pivotPointChanged)
            {
                if (pivotPointNull)
                {
                    Scene.SuspendDefer();
                    pivotPoint = ref Scene.PivotPointTable.Set(this, value.PivotPoint).Value;
                    Scene.ResumeDefer();
                }
                else
                {
                    pivotPoint.Value = value.PivotPoint;
                }
            }

            Scene.TransformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, value));
            if (positionChanged)
                Scene.PositionTable.Enqueue(Table.Event<Position>.Set(this, oldPosition, value.Position));
            if (scaleChanged)
                Scene.ScaleTable.Enqueue(Table.Event<Scale>.Set(this, oldScale, value.Scale));
            if (rotationChanged)
                Scene.RotationTable.Enqueue(Table.Event<Rotation>.Set(this, oldRotation, value.Rotation));
            if (pivotPointChanged)
                Scene.PivotPointTable.Enqueue(Table.Event<PivotPoint>.Set(this, oldPivotPoint, value.PivotPoint));
        }
    }

    public Vector2 Position
    {
        get
        {
            AssertValid();
            return Scene is null ? default : Scene.PositionTable.GetRef(this).GetOrDefault();
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var position = ref Scene.PositionTable.GetRef(this).Value;
            Vector2 oldPosition;
            if (Unsafe.IsNullRef(ref position))
            {
                Scene.SuspendDefer();
                position = ref Scene.PositionTable.Set(this, value).Value;
                oldPosition = new Position();
                Scene.ResumeDefer();
            }
            else
            {
                oldPosition = position;
                if (Precision.AreEqual(value, oldPosition))
                    return;
                position.Value = value;
            }

            ref var transform = ref Scene.TransformTable.GetRef(this).Value;
            Transform oldTransform;
            if (Unsafe.IsNullRef(ref transform))
            {
                Scene.SuspendDefer();
                transform = ref Scene.TransformTable.Set(this, new Transform { Position = value }).Value;
                oldTransform = new Transform();
                Scene.ResumeDefer();
            }
            else
            {
                oldTransform = transform;
                transform.Position = value;
            }

            Scene.PositionTable.Enqueue(Table.Event<Position>.Set(this, oldPosition, value));
            Scene.TransformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public Vector2 Scale
    {
        get
        {
            AssertValid();
            var scale = Scene is null ? ComponentRef<Scale>.Null : Scene.ScaleTable.GetRef(this);
            return scale.IsNull ? new Scale() : scale;
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var scale = ref Scene.ScaleTable.GetRef(this).Value;
            Scale oldScale;
            if (Unsafe.IsNullRef(ref scale))
            {
                Scene.SuspendDefer();
                scale = ref Scene.ScaleTable.Set(this, value).Value;
                oldScale = new Scale();
                Scene.ResumeDefer();
            }
            else
            {
                oldScale = scale;
                if (Precision.AreEqual(value, oldScale))
                    return;
                scale.Value = value;
            }

            ref var transform = ref Scene.TransformTable.GetRef(this).Value;
            Transform oldTransform;
            if (Unsafe.IsNullRef(ref transform))
            {
                Scene.SuspendDefer();
                transform = ref Scene.TransformTable.Set(this, new Transform { Scale = value }).Value;
                oldTransform = new Transform();
                Scene.ResumeDefer();
            }
            else
            {
                oldTransform = transform;
                transform.Scale = value;
            }

            Scene.ScaleTable.Enqueue(Table.Event<Scale>.Set(this, oldScale, value));
            Scene.TransformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public float Rotation
    {
        get
        {
            AssertValid();
            return Scene is null ? default : Scene.RotationTable.GetRef(this).GetOrDefault();
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var rotation = ref Scene.RotationTable.GetRef(this).Value;
            Rotation oldRotation;
            if (Unsafe.IsNullRef(ref rotation))
            {
                Scene.SuspendDefer();
                rotation = ref Scene.RotationTable.Set(this, value).Value;
                oldRotation = default;
                Scene.ResumeDefer();
            }
            else
            {
                oldRotation = rotation;
                if (Precision.AreEqual(value, oldRotation))
                    return;
                rotation.Value = value;
            }

            ref var transform = ref Scene.TransformTable.GetRef(this).Value;
            Transform oldTransform;
            if (Unsafe.IsNullRef(ref transform))
            {
                Scene.SuspendDefer();
                transform = ref Scene.TransformTable.Set(this, new Transform { Rotation = value }).Value;
                oldTransform = new Transform();
                Scene.ResumeDefer();
            }
            else
            {
                oldTransform = transform;
                transform.Rotation = value;
            }

            Scene.RotationTable.Enqueue(Table.Event<Rotation>.Set(this, oldRotation, value));
            Scene.TransformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public Vector2 PivotPoint
    {
        get
        {
            AssertValid();
            return Scene is null ? default : Scene.PivotPointTable.GetRef(this).GetOrDefault();
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var pivotPoint = ref Scene.PivotPointTable.GetRef(this).Value;
            PivotPoint oldPivotPoint;
            if (Unsafe.IsNullRef(ref pivotPoint))
            {
                Scene.SuspendDefer();
                pivotPoint = ref Scene.PivotPointTable.Set(this, value).Value;
                oldPivotPoint = default;
                Scene.ResumeDefer();
            }
            else
            {
                oldPivotPoint = pivotPoint;
                if (Precision.AreEqual(value, oldPivotPoint))
                    return;
                pivotPoint.Value = value;
            }

            ref var transform = ref Scene.TransformTable.GetRef(this).Value;
            Transform oldTransform;
            if (Unsafe.IsNullRef(ref transform))
            {
                Scene.SuspendDefer();
                transform = ref Scene.TransformTable.Set(this, new Transform { PivotPoint = value }).Value;
                oldTransform = new Transform();
                Scene.ResumeDefer();
            }
            else
            {
                oldTransform = transform;
                transform.PivotPoint = value;
            }

            Scene.PivotPointTable.Enqueue(Table.Event<PivotPoint>.Set(this, oldPivotPoint, value));
            Scene.TransformTable.Enqueue(Table.Event<Transform>.Set(this, oldTransform, transform));
        }
    }

    public int ZIndex
    {
        get
        {
            AssertValid();
            return Scene is null ? default : Scene.ZIndexTable.GetRef(this).GetOrDefault();
        }
        set
        {
            AssertValid();
            if (Scene is null)
                return;
            ref var zIndex = ref Scene.ZIndexTable.GetRef(this).Value;
            ZIndex oldZIndex;
            if (Unsafe.IsNullRef(ref zIndex))
            {
                Scene.SuspendDefer();
                zIndex = ref Scene.ZIndexTable.Set(this, value).Value;
                oldZIndex = default;
                Scene.ResumeDefer();
            }
            else
            {
                oldZIndex = zIndex;
                if (value == oldZIndex.Value)
                    return;
                zIndex.Value = value;
            }

            Scene.ZIndexTable.Enqueue(Table.Event<ZIndex>.Set(this, oldZIndex, value));
        }
    }

    public TableEnumerable Tables()
    {
        return new TableEnumerable(this);
    }

    public TableEnumerable<T> Tables<T>()
    {
        return new TableEnumerable<T>(this);
    }

    public ComponentEnumerable Components()
    {
        return new ComponentEnumerable(this);
    }

    public ChildEnumerable Children()
    {
        return new ChildEnumerable(this);
    }

    public T Get<T>()
    {
        AssertValid();
        return Scene is null ? Unsafe.NullRef<T>() : Scene.Table<T>().GetRef(this);
    }

    public object Get(Table table)
    {
        AssertValid();
        return table.Get(this);
    }

    public bool TryGet<T>(out T component)
    {
        AssertValid();
        Unsafe.SkipInit(out component);
        if (Scene is null)
            return false;
        var data = Scene.Table<T>().GetRef(this);
        if (data.IsNull)
            return false;
        component = data;
        return true;
    }

    public bool TryGetRef<T>(out ComponentRef<T> componentRef)
    {
        AssertValid();
        if (Scene is null)
        {
            componentRef = ComponentRef<T>.Null;
            return false;
        }

        componentRef = Scene.Table<T>().GetRef(this);
        return !componentRef.IsNull;
    }

    public bool TryGet(Table table, out object value)
    {
        AssertValid();
        return table.TryGet(this, out value);
    }

    // ReSharper disable once ReturnTypeCanBeNotNullable
    public T? GetOrDefault<T>()
    {
        AssertValid();
        if (Scene is null)
            return default;
        var value = Scene.Table<T>().GetRef(this);
        return value.IsNull ? default : value;
    }

    public T GetOrDefault<T>(in T defaultValue)
    {
        AssertValid();
        if (Scene is null)
            return defaultValue;
        var value = Scene.Table<T>().GetRef(this);
        return value.IsNull ? defaultValue : value;
    }

    public T GetOrDefault<T>(Func<T> defaultFunc)
    {
        AssertValid();
        if (Scene is null)
            return defaultFunc.Invoke();
        var value = Scene.Table<T>().GetRef(this);
        return value.IsNull ? defaultFunc.Invoke() : value;
    }

    public object? GetOrDefault(Table table)
    {
        AssertValid();
        return table.TryGet(this, out var value) ? value : null;
    }

    public object GetOrDefault(Table table, object defaultValue)
    {
        AssertValid();
        return table.TryGet(this, out var value) ? value : defaultValue;
    }

    public object GetOrDefault(Table table, Func<object> defaultValue)
    {
        AssertValid();
        return table.TryGet(this, out var value) ? value : defaultValue.Invoke();
    }

    public ComponentRef<T> GetRef<T>()
    {
        AssertValid();
        return Scene is null ? ComponentRef<T>.Null : Scene.Table<T>().GetRef(this);
    }

    [OverloadResolutionPriority(1)]
    [UnscopedRef]
    public ref readonly Entity Set<T>(IComposable<T> composable)
    {
        AssertValid();
        Set(composable.ToComponent());
        return ref this;
    }

    [OverloadResolutionPriority(1)]
    [UnscopedRef]
    public ref readonly Entity Set<T>(IComposable<T> composable, out ComponentRef<T> componentRef)
    {
        AssertValid();
        Set(composable.ToComponent(), out componentRef);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Set<T>()
        where T : new()
    {
        AssertValid();
        if (Scene is null)
            return ref this;
        Scene.Table<T>().Set(this, new T());
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Set<T>(out ComponentRef<T> componentRef)
        where T : new()
    {
        AssertValid();
        if (Scene is null)
        {
            componentRef = ComponentRef<T>.Null;
            return ref this;
        }

        componentRef = Scene.Table<T>().Set(this, new T());
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Set<T>(in T component)
    {
        AssertValid();
        if (Scene is null)
            return ref this;
        Scene.Table<T>().Set(this, component);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Set<T>(scoped in T component, out ComponentRef<T> componentRef)
    {
        AssertValid();
        if (Scene is null)
        {
            componentRef = ComponentRef<T>.Null;
            return ref this;
        }

        componentRef = Scene.Table<T>().Set(this, component);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Set(Table table, object value)
    {
        AssertValid();
        table.Set(this, value);
        return ref this;
    }

    public bool Remove<T>()
    {
        AssertValid();
        return Scene is not null && Scene.Table<T>().Remove(this);
    }

    public bool Remove<T>(out T component)
    {
        AssertValid();
        if (Scene is not null)
            return Scene.Table<T>().Remove(this, out component);
        component = default!;
        return false;
    }

    public bool Remove(Table table)
    {
        AssertValid();
        return table.Remove(this);
    }

    public bool Remove(Table table, out object component)
    {
        AssertValid();
        return table.Remove(this, out component);
    }

    public void Clear()
    {
        AssertValid();
        if (Scene is null)
            return;
        foreach (var table in Scene.Tables().WithHidden())
            table.Remove(this, Table.Flags.SilentOnImmutable);
    }

    public void Destroy()
    {
        AssertValid();
        Scene?.Destroy(this);
    }

    [Conditional("VIGILANCE_ASSERTS")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssertValid()
    {
        Debug.Assert(IsValid, "Entity must be valid.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new InvalidOperationException("Entity must be valid.");
    }

    [UnscopedRef]
    public ref readonly Entity SetTransform(in Transform transform)
    {
        Transform = transform;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetPosition(float v1, float? v2 = null)
    {
        Position = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetPosition(Vector2 position)
    {
        Position = position;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetScale(float v1, float? v2 = null)
    {
        Scale = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetScale(Vector2 scale)
    {
        Scale = scale;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetRotation(float rotation)
    {
        Rotation = rotation;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetPivotPoint(float v1, float? v2 = null)
    {
        PivotPoint = new Vector2(v1, v2 ?? v1);
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetPivotPoint(Vector2 pivotPoint)
    {
        PivotPoint = pivotPoint;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetZIndex(int zIndex)
    {
        ZIndex = zIndex;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetDisabled(bool disabled = true)
    {
        IsDisabled = disabled;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity SetParent(in Entity parent)
    {
        Parent = parent;
        return ref this;
    }

    [UnscopedRef]
    public ref readonly Entity Scope(Action action)
    {
        AssertValid();
        if (Scene is null)
        {
            action.Invoke();
            return ref this;
        }

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

    [UnscopedRef]
    public ref readonly Entity Scope(Action<Scene> action)
    {
        AssertValid();
        if (Scene is null)
            return ref this;
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
        if (Id == EntityId.Null)
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
        sb.Append(Components().ToString());
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

        [UnscopedRef]
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
            _entity.AssertValid();
            _enumerator = _entity.Scene.Tables().WithHidden(_withHidden).GetEnumerator();
            Current = null!;
        }

        public Table Current { get; private set; } = null!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }

    public struct TableEnumerable<T> : IStructEnumerable<TableEnumerator<T>, Table>
    {
        private readonly Entity _entity;
        private bool _withHidden;

        internal TableEnumerable(in Entity entity)
        {
            _entity = entity;
        }

        public TableEnumerator<T> GetEnumerator()
        {
            return new TableEnumerator<T>(_entity, _withHidden);
        }

        public ValueEnumerable<StructEnumerator<TableEnumerator<T>, Table>, Table> AsValueEnumerable()
        {
            return new StructEnumerator<TableEnumerator<T>, Table>(GetEnumerator());
        }

        [UnscopedRef]
        public ref TableEnumerable<T> WithHidden(bool withHidden = true)
        {
            _withHidden = withHidden;
            return ref this;
        }
    }

    public struct TableEnumerator<T> : IStructEnumerator<Table>
    {
        private readonly Entity _entity;
        private readonly bool _withHidden;
        private Scene.TableEnumerator<T> _enumerator;

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
            _entity.AssertValid();
            _enumerator = _entity.Scene.Tables<T>().WithHidden(_withHidden).GetEnumerator();
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

        [UnscopedRef]
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
            foreach (var table in _entity.Tables().WithHidden(_withHidden))
            {
                if (!table.TryGet(_entity, out var component))
                    continue;
                any = true;
                sb.Append($"\n [ {table.Type}, {component} ], ");
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
            _entity.AssertValid();
            _enumerator = _entity.Scene.Tables().WithHidden(_withHidden).GetEnumerator();
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

        [UnscopedRef]
        public ref ChildEnumerable Deferred(bool deferred = true)
        {
            _deferred = deferred;
            return ref this;
        }
    }

    public struct ChildEnumerator : IStructEnumerator<Entity>
    {
        private readonly Entity _parent;
        private EntityId _nextChildId;
        private readonly bool _deferred;
        private bool _initialized;
        private bool _disposed;

        internal ChildEnumerator(in Entity parent, bool deferred)
        {
            _parent = parent;
            _deferred = deferred;
            _initialized = false;
            _disposed = true;
        }

        private void Initialize()
        {
            _parent.AssertValid();
            var parentRef = _parent.Scene.ParentTable.GetRef(_parent);
            _nextChildId = parentRef.IsNull ? EntityId.Null : parentRef.Read.FirstChildId;
            Current = Null;
            _initialized = true;
            _disposed = false;
            if (_deferred)
                _parent.Scene.BeginDefer();
        }

        public bool MoveNext()
        {
            if (!_initialized)
                Initialize();
            if (_nextChildId == EntityId.Null)
            {
                Current = default;
                return false;
            }

            Current = new Entity(_nextChildId, _parent.Scene);
            var childRef = _parent.Scene.ChildTable.GetRef(Current);
            _nextChildId = childRef.IsNull ? EntityId.Null : childRef.Read.NextSiblingId;
            return true;
        }

        public void Reset()
        {
            Dispose();
            _initialized = false;
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
        private EntityId _nextChildId;
        private EntityId _nextSiblingId;
        private EntityId _previousSiblingId;
        private readonly bool _deferred;
        private bool _hasDeferBegun;
        private bool _childrenInitialized;
        private bool _nextSiblingInitialized;
        private bool _previousSiblingInitialized;

        public Entity Origin { get; }

        internal Traverser(in Entity origin, bool deferred = true)
        {
            origin.AssertValid();
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
            hasChild = Origin.Scene.ParentTable.Has(Origin);
            return true;
        }

        public bool TryGetParent(out Entity parent)
        {
            parent = Origin.Parent;
            return !parent.IsNull;
        }

        public bool TryGetNextChild(out Entity child)
        {
            EnsureDeferred();
            if (!_childrenInitialized)
            {
                var parentRef = Origin.Scene.ParentTable.GetRef(Origin);
                _nextChildId = parentRef.IsNull ? EntityId.Null : parentRef.Read.FirstChildId;
                _childrenInitialized = true;
            }

            if (_nextChildId == EntityId.Null)
            {
                child = Null;
                return false;
            }

            child = new Entity(_nextChildId, Origin.Scene);
            var childRef = child.Scene.ChildTable.GetRef(child);
            _nextChildId = childRef.IsNull ? EntityId.Null : childRef.Read.NextSiblingId;
            return true;
        }

        public bool TryGetNextSibling(out Entity next)
        {
            EnsureDeferred();
            if (!_nextSiblingInitialized)
            {
                var childRef = Origin.Scene.ChildTable.GetRef(Origin);
                _nextSiblingId = childRef.IsNull ? EntityId.Null : childRef.Read.NextSiblingId;
                _nextSiblingInitialized = true;
            }

            if (_nextSiblingId == EntityId.Null)
            {
                next = Null;
                return false;
            }

            next = new Entity(_nextSiblingId, Origin.Scene);
            var nextChildRef = next.Scene.ChildTable.GetRef(next);
            _nextSiblingId = nextChildRef.IsNull ? EntityId.Null : nextChildRef.Read.NextSiblingId;
            return true;
        }

        public bool TryGetPreviousSibling(out Entity previous)
        {
            EnsureDeferred();
            if (!_previousSiblingInitialized)
            {
                var childRef = Origin.Scene.ChildTable.GetRef(Origin);
                _previousSiblingId = childRef.IsNull ? EntityId.Null : childRef.Read.PreviousSiblingId;
                _previousSiblingInitialized = true;
            }

            if (_previousSiblingId == EntityId.Null)
            {
                previous = Null;
                return false;
            }

            previous = new Entity(_previousSiblingId, Origin.Scene);
            var prevChildRef = previous.Scene.ChildTable.GetRef(previous);
            _previousSiblingId = prevChildRef.IsNull ? EntityId.Null : prevChildRef.Read.PreviousSiblingId;
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

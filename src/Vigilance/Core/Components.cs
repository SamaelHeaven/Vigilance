#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

using System.Runtime.InteropServices;
using Flecs.NET.Bindings;
using LinkDotNet.StringBuilder;
using ZLinq;

namespace Vigilance.Core;

public unsafe struct Components : IStructEnumerable<Components.Enumerator, Component>
{
    private readonly flecs.ecs_type_t* _type;
    private bool _deferred = true;

    public static Components Empty => new(Entity.Null);

    public Entity Entity { get; }

    internal Components(Entity entity)
    {
        Entity = entity;
        _type = entity.IsValid ? flecs.ecs_get_type(entity.Scene.World, entity.Id) : null;
    }

    public ref Components Deferred(bool deferred = true)
    {
        _deferred = deferred;
        return ref this;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(ref this);
    }

    public ValueEnumerable<StructEnumerator<Enumerator, Component>, Component> AsValueEnumerable()
    {
        return new StructEnumerator<Enumerator, Component>(GetEnumerator());
    }

    public override string ToString()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append('[');
        var any = false;
        foreach (var component in this)
        {
            any = true;
            sb.Append($"\n {Entity.Get(component)}, ");
        }

        if (any)
            sb.Append('\n');
        sb.Append(']');
        return sb.ToString();
    }

    public struct Enumerator : IStructEnumerator<Component>
    {
        private readonly Components _components;
        private bool _valid;
        private int _index;

        public Component Current { get; private set; }

        internal Enumerator(ref Components components)
        {
            _components = components;
            Reset();
        }

        public bool MoveNext()
        {
            if (!_valid)
                return false;
            if (_components._type == null)
                return false;
            var scene = _components.Entity.Scene;
            while (++_index < _components._type->count)
            {
                var id = _components._type->array[_index];
                if (
                    id == scene.Cache.PositionId
                    || id == scene.Cache.ScaleId
                    || id == scene.Cache.RotationId
                    || id == scene.Cache.PivotPointId
                    || id == scene.Cache.ZIndexId
                    || id >= long.MaxValue
                )
                    continue;
                ref var type = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    scene.Cache.ComponentMap,
                    id,
                    out var exists
                );
                if (!exists)
                {
                    var entity = new Flecs.NET.Core.Entity(scene.World, id);
                    type = entity.Get<Type>();
                }

                Current = new Component(id, scene, type!);
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _components.Entity.EnsureValid();
            _valid = _components.Entity.IsValid;
            if (_valid && _components._deferred)
                _components.Entity.Scene.BeginDefer();
            _index = -1;
        }

        public void Dispose()
        {
            if (_valid && _components._deferred)
                _components.Entity.Scene.EndDefer();
        }
    }
}

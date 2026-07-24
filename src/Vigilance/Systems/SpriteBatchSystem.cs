using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Systems;

public sealed class SpriteBatchSystem : GameSystem
{
    private ValueDictionary<SpriteBatch, ValueEntitySparseSet<SpriteInstance, SpriteBatch>> _batches = [];
    private ValueEntitySparseSet _moving;
    private Table<BatchedSprite> _table = null!;

    public override void Configure()
    {
        _moving = new ValueEntitySparseSet(Scene);
        _table = Scene.Table<BatchedSprite>();
        Scene.OnAdd<BatchedSprite>(UpdateSprite);
        Scene.OnSet<BatchedSprite>(SetSprite);
        Scene.OnRemove<BatchedSprite>(RemoveSprite);
        Scene.OnSet<Interpolation>(OnInterpolationChanged);
        Scene.OnAddOrSet<Child>(TryUpdateSprite);
        Scene.OnRemove<Child>(TryUpdateSprite);
    }

    public override void PreRender()
    {
        for (var i = _moving.Count - 1; i >= 0; i--)
        {
            var entity = _moving[i];
            if (entity.IsValid && _table.TryGet(entity, out var sprite))
                UpdateSprite(entity, sprite);
            else
                _moving.Remove(entity);
        }
    }

    private void OnInterpolationChanged(Entity entity, Interpolation interpolation)
    {
        var moving = interpolation.Start.HasValue && !Precision.AreEqual(interpolation.Start, interpolation.End);
        Track(entity, moving);
        if (!entity.IsParent)
            return;
        foreach (var child in entity.Descendants())
            Track(child, moving);
    }

    private void Track(in Entity entity, bool moving)
    {
        if (!_table.TryGet(entity, out var sprite))
            return;
        UpdateSprite(entity, sprite);
        if (moving)
            _moving.Add(entity);
        else
            _moving.Remove(entity);
    }

    private void TryUpdateSprite(Entity entity, Child child)
    {
        if (_table.TryGet(entity, out var sprite))
            UpdateSprite(entity, sprite);
        if (!entity.IsParent)
            return;
        foreach (var descendant in entity.Descendants())
            if (descendant.TryGet(out sprite))
                UpdateSprite(descendant, sprite);
    }

    private void UpdateSprite(Entity entity, BatchedSprite sprite)
    {
        ref var instances = ref _batches.GetValueRefOrAddDefault(sprite.Batch, out var exists)!;
        if (!exists)
            instances = new ValueEntitySparseSet<SpriteInstance, SpriteBatch>(Scene, sprite.Batch);
        instances[entity] = sprite.Instance with { Transform = sprite.Instance.Transform + entity.RenderTransform };
    }

    private void SetSprite(Entity entity, BatchedSprite oldSprite, BatchedSprite newSprite)
    {
        if (oldSprite.Batch != newSprite.Batch)
            RemoveSprite(entity, oldSprite);
        UpdateSprite(entity, newSprite);
    }

    private void RemoveSprite(Entity entity, BatchedSprite sprite)
    {
        _moving.Remove(entity);
        ref var instances = ref _batches.GetValueRefOrNullRef(sprite.Batch);
        if (Unsafe.IsNullRef(ref instances))
            return;
        instances.Remove(entity);
        if (instances.Count == 0)
            _batches.Remove(sprite.Batch);
    }
}

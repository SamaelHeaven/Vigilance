using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Systems;

public sealed class SpriteBatchSystem : GameSystem
{
    private ValueDictionary<SpriteBatch, ValueSparseSet<ulong, SpriteInstance, SpriteBatch>> _batches = [];
    private ValueSparseSet<ulong, byte, ValueList<byte>> _moving = new([], Entity.GetIndex);
    private Table<BatchedSprite> _table = null!;

    public override void Initialize()
    {
        _table = Scene.Table<BatchedSprite>();
    }

    public override void Configure()
    {
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
            var (entityId, _) = _moving[i];
            var entity = new Entity(entityId, Scene);
            if (entity.IsValid && _table.TryGet(entity, out var sprite))
                UpdateSprite(entity, sprite);
            else
                _moving.Remove(entityId);
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
            _moving[entity.Id] = 0;
        else
            _moving.Remove(entity.Id);
    }

    private void TryUpdateSprite(Entity entity)
    {
        if (_table.TryGet(entity, out var sprite))
            UpdateSprite(entity, sprite);
        if (!entity.IsParent)
            return;
        foreach (var child in entity.Descendants())
            if (child.TryGet(out sprite))
                UpdateSprite(child, sprite);
    }

    private void UpdateSprite(Entity entity, BatchedSprite sprite)
    {
        ref var instances = ref _batches.GetValueRefOrAddDefault(sprite.Batch, out var exists)!;
        if (!exists)
            instances = new ValueSparseSet<ulong, SpriteInstance, SpriteBatch>(sprite.Batch, Entity.GetIndex);
        instances[entity.Id] = sprite.Instance with { Transform = sprite.Instance.Transform + entity.RenderTransform };
    }

    private void SetSprite(Entity entity, BatchedSprite oldSprite, BatchedSprite newSprite)
    {
        if (oldSprite.Batch != newSprite.Batch)
            RemoveSprite(entity, oldSprite);
        UpdateSprite(entity, newSprite);
    }

    private void RemoveSprite(Entity entity, BatchedSprite sprite)
    {
        _moving.Remove(entity.Id);
        ref var instances = ref _batches.GetValueRefOrNullRef(sprite.Batch);
        if (Unsafe.IsNullRef(ref instances))
            return;
        instances.Remove(entity.Id);
        if (instances.Count == 0)
            _batches.Remove(sprite.Batch);
    }
}

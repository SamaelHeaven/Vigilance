using System.Runtime.CompilerServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using ZLinq;

namespace Vigilance.Systems;

public sealed class SpriteBatchSystem : GameSystem
{
    private ValueDictionary<SpriteBatch, ValueSparseSet<Entity, SpriteInstance, SpriteBatch>> _batches = [];

    public override void Configure()
    {
        Scene.OnAdd<BatchedSprite>(UpdateSprite);
        Scene.OnSet<BatchedSprite>(SetSprite);
        Scene.OnRemove<BatchedSprite>(RemoveSprite);
        Scene.OnSet<Interpolation>(TryUpdateSprite);
        Scene.OnAddOrSet<Child>(TryUpdateSprite);
        Scene.OnRemove<Child>(TryUpdateSprite);
    }

    private void TryUpdateSprite(Entity entity)
    {
        if (entity.TryGet(out BatchedSprite sprite))
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
            instances = new ValueSparseSet<Entity, SpriteInstance, SpriteBatch>(sprite.Batch, e => e.Index);
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
        ref var instances = ref _batches.GetValueRefOrNullRef(sprite.Batch);
        if (!Unsafe.IsNullRef(ref instances))
            return;
        instances.Remove(entity);
        if (instances.Count == 0)
            _batches.Remove(sprite.Batch);
    }
}

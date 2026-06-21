using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Systems;

public sealed class SpriteBatchSystem : GameSystem
{
    private readonly Dictionary<SpriteBatch, SparseSet<Entity, SpriteInstance, SpriteBatch>> _batches = new();

    public override void Configure()
    {
        Scene.OnAdd<BatchedSprite>(UpdateSprite);
        Scene.OnSet<BatchedSprite>(SetSprite);
        Scene.OnRemove<BatchedSprite>(RemoveSprite);
        Scene.OnSet<Transform>(TryUpdateSprite);
        Scene.OnAddOrSet<Child>(TryUpdateSprite);
        Scene.OnRemove<Child>(TryUpdateSprite);
    }

    private void TryUpdateSprite(Entity entity)
    {
        foreach (var child in entity.DescendantsAndSelf())
            if (child.TryGet(out BatchedSprite sprite))
                UpdateSprite(child, sprite);
    }

    private void UpdateSprite(Entity entity, BatchedSprite sprite)
    {
        ref var instances = ref CollectionsMarshal.GetValueRefOrAddDefault(_batches, sprite.Batch, out var exists)!;
        if (!exists)
            instances = new SparseSet<Entity, SpriteInstance, SpriteBatch>(sprite.Batch, e => e.Index);
        instances.Set(entity, sprite.Instance with { Transform = sprite.Instance.Transform + entity.WorldTransform });
    }

    private void SetSprite(Entity entity, BatchedSprite oldSprite, BatchedSprite newSprite)
    {
        if (oldSprite.Batch != newSprite.Batch)
        {
            RemoveSprite(entity, oldSprite);
            return;
        }

        UpdateSprite(entity, newSprite);
    }

    private void RemoveSprite(Entity entity, BatchedSprite sprite)
    {
        if (!_batches.TryGetValue(sprite.Batch, out var instances))
            return;
        instances.Remove(entity);
        if (instances.Count == 0)
            _batches.Remove(sprite.Batch);
    }
}

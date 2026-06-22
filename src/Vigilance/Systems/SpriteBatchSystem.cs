using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Systems;

public sealed class SpriteBatchSystem : GameSystem
{
    private readonly Dictionary<SpriteBatch, EntitySparseSet<SpriteInstance, SpriteBatch>> _batches = new();

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
        ref var instances = ref CollectionsMarshal.GetValueRefOrAddDefault(_batches, sprite.Batch, out var exists)!;
        if (!exists)
            instances = new EntitySparseSet<SpriteInstance, SpriteBatch>(sprite.Batch);
        instances[entity] = sprite.Instance with { Transform = sprite.Instance.Transform + entity.WorldTransform };
    }

    private void SetSprite(Entity entity, BatchedSprite oldSprite, BatchedSprite newSprite)
    {
        if (oldSprite.Batch != newSprite.Batch)
            RemoveSprite(entity, oldSprite);
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

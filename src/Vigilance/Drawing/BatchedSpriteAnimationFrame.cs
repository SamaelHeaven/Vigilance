using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct BatchedSpriteAnimationFrame : IAnimationFrame
{
    public Wrapper<Box?>? Source { get; set; }
    public TimeSpan Delay { get; set; }
    public Vector2? Position { get; set; }
    public Vector2? Scale { get; set; }
    public Vector2? PivotPoint { get; set; }
    public Color? Tint { get; set; }
    public float? Rotation { get; set; }
    public bool? FlipX { get; set; }
    public bool? FlipY { get; set; }

    public Transform Transform
    {
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public readonly void Apply(Entity entity)
    {
        if (!entity.TryGet(out BatchedSprite sprite))
            return;
        var newSprite = sprite;
        Apply(ref newSprite);
        if (sprite != newSprite)
            entity.Set(newSprite);
    }

    public readonly void Apply(ref BatchedSprite sprite)
    {
        var instance = sprite.Instance;
        Apply(ref instance);
        sprite.Instance = instance;
    }

    public readonly void Apply(ref SpriteInstance sprite)
    {
        if (FlipX.HasValue)
            sprite.FlipX = FlipX.Value;
        if (FlipY.HasValue)
            sprite.FlipY = FlipY.Value;
        if (Source.HasValue)
            sprite.Source = Source;
        if (Tint.HasValue)
            sprite.Tint = Tint.Value;
        if (Position.HasValue)
            sprite.Position = Position.Value;
        if (Scale.HasValue)
            sprite.Scale = Scale.Value;
        if (Rotation.HasValue)
            sprite.Rotation = Rotation.Value;
        if (PivotPoint.HasValue)
            sprite.PivotPoint = PivotPoint.Value;
    }
}

public static class TextureAtlasBatchedSpriteAnimationExtensions
{
    extension(TextureAtlas atlas)
    {
        public BatchedSpriteAnimationFrameEnumerable GetBatchedSpriteAnimationFrames(
            int startCol,
            int startRow,
            int endCol,
            int? endRow = null
        )
        {
            return new BatchedSpriteAnimationFrameEnumerable(atlas.GetRegions(startCol, startRow, endCol, endRow));
        }

        public BatchedSpriteAnimationFrameEnumerable GetBatchedSpriteAnimationFrames(
            Vector2 startPosition,
            Vector2 endPosition
        )
        {
            return new BatchedSpriteAnimationFrameEnumerable(atlas.GetRegions(startPosition, endPosition));
        }

        public BatchedSpriteAnimationFrameEnumerable GetBatchedSpriteAnimationFrames(int startIndex, int endIndex)
        {
            return new BatchedSpriteAnimationFrameEnumerable(atlas.GetRegions(startIndex, endIndex));
        }
    }

    public readonly struct BatchedSpriteAnimationFrameEnumerable
        : IStructEnumerable<BatchedSpriteAnimationFrameEnumerable.Enumerator, BatchedSpriteAnimationFrame>,
            IReadOnlyCollection<BatchedSpriteAnimationFrame>
    {
        private readonly TextureAtlas.RegionEnumerable _regions;

        public BatchedSpriteAnimationFrameEnumerable(TextureAtlas.RegionEnumerable regions)
        {
            _regions = regions;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_regions.GetEnumerator());
        }

        public ValueEnumerable<
            StructEnumerator<Enumerator, BatchedSpriteAnimationFrame>,
            BatchedSpriteAnimationFrame
        > AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, BatchedSpriteAnimationFrame>(GetEnumerator());
        }

        public int Count => _regions.Count;

        public struct Enumerator : IStructEnumerator<BatchedSpriteAnimationFrame>
        {
            private TextureAtlas.RegionEnumerable.Enumerator _regions;

            internal Enumerator(TextureAtlas.RegionEnumerable.Enumerator regions)
            {
                _regions = regions;
                Current = default!;
            }

            public bool MoveNext()
            {
                if (!_regions.MoveNext())
                    return false;
                Current = new BatchedSpriteAnimationFrame { Source = _regions.Current };
                return true;
            }

            public void Reset()
            {
                _regions.Reset();
                Current = default!;
            }

            public BatchedSpriteAnimationFrame Current { get; private set; }

            public void Dispose() { }
        }
    }
}

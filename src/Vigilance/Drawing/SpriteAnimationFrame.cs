using System.Runtime.InteropServices;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public record struct SpriteAnimationFrame : IAnimationFrame
{
    public Texture? Texture { get; set; }
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
        if (entity.TryGet(out Sprite sprite))
            Apply(sprite);
    }

    public readonly void Apply(Sprite sprite)
    {
        if (Texture is not null)
            sprite.Texture = Texture;
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

public static class TextureAtlasSpriteAnimationExtensions
{
    extension(TextureAtlas atlas)
    {
        public SpriteAnimationFrameEnumerable GetSpriteAnimationFrames(
            int startCol,
            int startRow,
            int endCol,
            int? endRow = null
        )
        {
            return new SpriteAnimationFrameEnumerable(atlas, atlas.GetRegions(startCol, startRow, endCol, endRow));
        }

        public SpriteAnimationFrameEnumerable GetSpriteAnimationFrames(Vector2 startPosition, Vector2 endPosition)
        {
            return new SpriteAnimationFrameEnumerable(atlas, atlas.GetRegions(startPosition, endPosition));
        }

        public SpriteAnimationFrameEnumerable GetSpriteAnimationFrames(int startIndex, int endIndex)
        {
            return new SpriteAnimationFrameEnumerable(atlas, atlas.GetRegions(startIndex, endIndex));
        }
    }

    public readonly struct SpriteAnimationFrameEnumerable
        : IStructEnumerable<SpriteAnimationFrameEnumerable.Enumerator, SpriteAnimationFrame>,
            IReadOnlyCollection<SpriteAnimationFrame>
    {
        private readonly Texture _texture;
        private readonly TextureAtlas.RegionEnumerable _regions;

        internal SpriteAnimationFrameEnumerable(TextureAtlas atlas, TextureAtlas.RegionEnumerable regions)
        {
            _texture = atlas.Texture;
            _regions = regions;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_texture, _regions.GetEnumerator());
        }

        public ValueEnumerable<
            StructEnumerator<Enumerator, SpriteAnimationFrame>,
            SpriteAnimationFrame
        > AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, SpriteAnimationFrame>(GetEnumerator());
        }

        public int Count => _regions.Count;

        public struct Enumerator : IStructEnumerator<SpriteAnimationFrame>
        {
            private readonly Texture _texture;
            private TextureAtlas.RegionEnumerable.Enumerator _regions;

            internal Enumerator(Texture texture, TextureAtlas.RegionEnumerable.Enumerator regions)
            {
                _texture = texture;
                _regions = regions;
                Current = default!;
            }

            public bool MoveNext()
            {
                if (!_regions.MoveNext())
                    return false;
                Current = new SpriteAnimationFrame { Texture = _texture, Source = _regions.Current };
                return true;
            }

            public void Reset()
            {
                _regions.Reset();
                Current = default!;
            }

            public SpriteAnimationFrame Current { get; private set; }

            public void Dispose() { }
        }
    }
}

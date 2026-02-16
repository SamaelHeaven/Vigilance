using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class TextureAtlas : IListView<Box>, IReadOnlyList<Box>
{
    private readonly List<Box> _boxes;

    public TextureAtlas(Texture texture, Vector2 count, float spacing = 0)
        : this(texture, (int)count.X, (int)count.Y, spacing) { }

    public TextureAtlas(Texture texture, int cols, int rows, float spacing = 0)
        : this(texture, texture.Width / (float)cols, texture.Height / (float)rows, cols * rows, spacing) { }

    public TextureAtlas(Texture texture, Vector2 regionSize, int count, float spacing = 0)
        : this(texture, regionSize.X, regionSize.Y, count, spacing) { }

    public TextureAtlas(Texture texture, float regionWidth, float regionHeight, int count, float spacing = 0)
    {
        var boxes = new List<Box>(count);
        Texture = texture;
        RegionSize = new Vector2(regionWidth, regionHeight);
        Spacing = spacing;
        var offsetX = 0.0f;
        var offsetY = 0.0f;
        for (var i = 0; i < count; i++)
        {
            boxes.Add(new Box(offsetX, offsetY, regionWidth, regionHeight));
            offsetX += regionWidth + spacing;
            if (!(offsetX + regionWidth > texture.Width))
                continue;
            offsetX = 0;
            offsetY += regionHeight + spacing;
        }

        _boxes = boxes;
    }

    public Texture Texture { get; }
    public Vector2 RegionSize { get; }
    public float Spacing { get; }

    public int Cols => (int)(TextureWidth / (RegionWidth + Spacing));

    public int Rows => (int)(Count / (float)Cols).Ceil();

    public Vector2 TextureSize => Texture.Size;

    public float TextureWidth => TextureSize.X;

    public float TextureHeight => TextureSize.Y;

    public float RegionWidth => RegionSize.X;

    public float RegionHeight => RegionSize.Y;

    public Box this[int x, int y] => GetRegion(x, y);

    public Box this[Vector2 position] => GetRegion(position);

    public List<Box>.Enumerator GetEnumerator()
    {
        return _boxes.GetEnumerator();
    }

    public ValueEnumerable<FromList<Box>, Box> AsValueEnumerable()
    {
        return _boxes.AsValueEnumerable();
    }

    public int Count => _boxes.Count;

    public Box this[int index] => GetRegion(index);

    public Box GetRegion(int index)
    {
        return _boxes[index];
    }

    public Box GetRegion(int col, int row)
    {
        return _boxes[GetIndex(col, row)];
    }

    public Box GetRegion(Vector2 position)
    {
        return _boxes[GetIndex(position)];
    }

    public int GetIndex(Vector2 position)
    {
        return GetIndex((int)position.X, (int)position.Y);
    }

    public int GetIndex(int col, int row)
    {
        return row * Cols + col;
    }

    public AnimationFrameEnumerable GetAnimationFrames(int startCol, int startRow, int endCol, int? endRow = null)
    {
        return GetAnimationFrames(GetIndex(startCol, startRow), GetIndex(endCol, endRow ?? startRow));
    }

    public AnimationFrameEnumerable GetAnimationFrames(Vector2 startPosition, Vector2 endPosition)
    {
        return GetAnimationFrames(GetIndex(startPosition), GetIndex(endPosition));
    }

    public AnimationFrameEnumerable GetAnimationFrames(int startIndex, int endIndex)
    {
        return new AnimationFrameEnumerable(this, startIndex, endIndex);
    }

    public readonly struct AnimationFrameEnumerable
        : IStructEnumerable<AnimationFrameEnumerator, AnimationFrame>,
            IReadOnlyCollection<AnimationFrame>
    {
        private readonly TextureAtlas _atlas;
        private readonly int _startIndex;
        private readonly int _endIndex;

        internal AnimationFrameEnumerable(TextureAtlas atlas, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex >= atlas.Count || startIndex > endIndex)
                throw new ArgumentException("Invalid animation index range.");
            _atlas = atlas;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }

        public AnimationFrameEnumerator GetEnumerator()
        {
            return new AnimationFrameEnumerator(_atlas, _startIndex, _endIndex);
        }

        public ValueEnumerable<
            StructEnumerator<AnimationFrameEnumerator, AnimationFrame>,
            AnimationFrame
        > AsValueEnumerable()
        {
            return new StructEnumerator<AnimationFrameEnumerator, AnimationFrame>(GetEnumerator());
        }

        public int Count => _endIndex - _startIndex + 1;
    }

    public struct AnimationFrameEnumerator : IStructEnumerator<AnimationFrame>
    {
        private readonly TextureAtlas _atlas;
        private readonly int _startIndex;
        private readonly int _endIndex;
        private int _currentIndex;

        internal AnimationFrameEnumerator(TextureAtlas atlas, int startIndex, int endIndex)
        {
            _atlas = atlas;
            _startIndex = startIndex;
            _currentIndex = startIndex - 1;
            _endIndex = endIndex;
            Current = null!;
        }

        public bool MoveNext()
        {
            if (_currentIndex >= _endIndex)
                return false;
            _currentIndex++;
            Current = new AnimationFrame { Texture = _atlas.Texture, Source = _atlas._boxes[_currentIndex] };
            return true;
        }

        public void Reset()
        {
            _currentIndex = _startIndex - 1;
            Current = null!;
        }

        public AnimationFrame Current { get; private set; }

        public void Dispose() { }
    }
}

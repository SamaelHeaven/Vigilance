using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Drawing;

public sealed class TextureAtlas : IArrayView<Box>
{
    private readonly Box[] _boxes;

    public TextureAtlas(Texture texture, Vector2 count, float spacing = 0)
        : this(texture, (int)count.X, (int)count.Y, spacing) { }

    public TextureAtlas(Texture texture, int cols, int rows, float spacing = 0)
        : this(texture, texture.Width / (float)cols, texture.Height / (float)rows, cols * rows, spacing) { }

    public TextureAtlas(Texture texture, Vector2 regionSize, int count, float spacing = 0)
        : this(texture, regionSize.X, regionSize.Y, count, spacing) { }

    public TextureAtlas(Texture texture, float regionWidth, float regionHeight, int count, float spacing = 0)
    {
        var boxes = GC.AllocateUninitializedArray<Box>(count);
        Texture = texture;
        RegionSize = new Vector2(regionWidth, regionHeight);
        Spacing = spacing;
        var offsetX = 0.0f;
        var offsetY = 0.0f;
        for (var i = 0; i < count; i++)
        {
            boxes[i] = new Box(offsetX, offsetY, regionWidth, regionHeight);
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

    public ArrayEnumerator<Box> GetEnumerator()
    {
        return _boxes;
    }

    public ValueEnumerable<FromArray<Box>, Box> AsValueEnumerable()
    {
        return _boxes.AsValueEnumerable();
    }

    public int Count => _boxes.Length;

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

    public RegionEnumerable GetRegions(int startCol, int startRow, int endCol, int? endRow = null)
    {
        return GetRegions(GetIndex(startCol, startRow), GetIndex(endCol, endRow ?? startRow));
    }

    public RegionEnumerable GetRegions(Vector2 startPosition, Vector2 endPosition)
    {
        return GetRegions(GetIndex(startPosition), GetIndex(endPosition));
    }

    public RegionEnumerable GetRegions(int startIndex, int endIndex)
    {
        return new RegionEnumerable(this, startIndex, endIndex);
    }

    public readonly struct RegionEnumerable
        : IStructEnumerable<RegionEnumerable.Enumerator, Box>,
            IReadOnlyCollection<Box>
    {
        private readonly TextureAtlas _atlas;
        private readonly int _startIndex;
        private readonly int _endIndex;

        internal RegionEnumerable(TextureAtlas atlas, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex >= atlas.Count || startIndex > endIndex)
                throw new ArgumentException("Invalid region index range.");
            _atlas = atlas;
            _startIndex = startIndex;
            _endIndex = endIndex;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_atlas, _startIndex, _endIndex);
        }

        public ValueEnumerable<Enumerator, Box> AsValueEnumerable()
        {
            return new ValueEnumerable<Enumerator, Box>(GetEnumerator());
        }

        ValueEnumerable<StructEnumerator<Enumerator, Box>, Box> IStructEnumerable<Enumerator, Box>.AsValueEnumerable()
        {
            return new StructEnumerator<Enumerator, Box>(GetEnumerator());
        }

        public int Count => _endIndex - _startIndex + 1;

        public struct Enumerator : IStructEnumerator<Box>, IValueEnumerator<Box>
        {
            private readonly TextureAtlas _atlas;
            private readonly int _startIndex;
            private readonly int _endIndex;
            private int _currentIndex;

            internal Enumerator(TextureAtlas atlas, int startIndex, int endIndex)
            {
                _atlas = atlas;
                _startIndex = startIndex;
                _currentIndex = startIndex - 1;
                _endIndex = endIndex;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_currentIndex >= _endIndex)
                    return false;
                _currentIndex++;
                Current = _atlas._boxes[_currentIndex];
                return true;
            }

            public bool TryGetNext(out Box current)
            {
                if (MoveNext())
                {
                    current = Current;
                    return true;
                }

                current = default;
                return false;
            }

            public bool TryGetNonEnumeratedCount(out int count)
            {
                count = _endIndex - _startIndex + 1;
                return true;
            }

            public bool TryGetSpan(out ReadOnlySpan<Box> span)
            {
                span = _atlas._boxes.AsSpan(_startIndex, _endIndex - _startIndex + 1);
                return true;
            }

            public bool TryCopyTo(scoped Span<Box> destination, Index offset)
            {
                var source = _atlas._boxes.AsSpan(_startIndex, _endIndex - _startIndex + 1);
                var start = offset.GetOffset(source.Length);
                if (start < 0 || start >= source.Length)
                    return false;
                source = source[start..];
                if (destination.Length < source.Length)
                    return false;
                source.CopyTo(destination);
                return true;
            }

            public void Reset()
            {
                _currentIndex = _startIndex - 1;
                Current = default;
            }

            public Box Current { get; private set; }

            public void Dispose() { }
        }
    }
}

using System.Collections;
using Vigilance.Math;

namespace Vigilance.Drawing;

public readonly struct TextureAtlas : IEnumerable<Box>
{
    private readonly Box[] _boxes;
    public Texture Texture { get; }
    public Vector2 RegionSize { get; }
    public float Spacing { get; }

    public int Cols => (int)(TextureWidth / (RegionWidth + Spacing));

    public int Rows => (int)MathF.Ceiling(Count / (float)Cols);

    public Vector2 TextureSize => Texture.Size;

    public float TextureWidth => TextureSize.X;

    public float TextureHeight => TextureSize.Y;

    public float RegionWidth => RegionSize.X;

    public float RegionHeight => RegionSize.Y;

    public Box this[int index] => GetRegion(index);

    public Box this[int x, int y] => GetRegion(x, y);

    public Box this[Vector2 position] => GetRegion(position);

    public int Count => _boxes.Length;

    public TextureAtlas(Texture texture, Vector2 count, float spacing = 0)
        : this(texture, (int)count.X, (int)count.Y, spacing) { }

    public TextureAtlas(Texture texture, int cols, int rows, float spacing = 0)
        : this(texture, texture.Width / (float)cols, texture.Height / (float)rows, cols * rows, spacing) { }

    public TextureAtlas(Texture texture, Vector2 regionSize, int count, float spacing = 0)
        : this(texture, regionSize.X, regionSize.Y, count, spacing) { }

    public TextureAtlas(Texture texture, float regionWidth, float regionHeight, int count, float spacing = 0)
    {
        _boxes = new Box[count];
        Texture = texture;
        RegionSize = new Vector2(regionWidth, regionHeight);
        Spacing = spacing;
        var offsetX = 0.0f;
        var offsetY = 0.0f;
        for (var i = 0; i < count; i++)
        {
            var box = new Box(offsetX, offsetY, regionWidth, regionHeight);
            _boxes[i] = box;
            offsetX += regionWidth + spacing;
            if (!(offsetX + regionWidth > texture.Width))
                continue;
            offsetX = 0;
            offsetY += regionHeight + spacing;
        }
    }

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

    public IEnumerator<Box> GetEnumerator()
    {
        return _boxes.Cast<Box>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

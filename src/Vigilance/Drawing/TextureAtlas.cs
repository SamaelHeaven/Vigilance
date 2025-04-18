using System.Collections;
using Vigilance.Math;

namespace Vigilance.Drawing;

public readonly struct TextureAtlas : IEnumerable<Box>
{
    private readonly List<Box> _boxes;
    public Texture Texture { get; }
    public Vector2 RegionSize { get; }
    public float Spacing { get; }

    public Vector2 Size => Texture.Size;

    public float Width => Size.X;

    public float Height => Size.Y;

    public float RegionWidth => RegionSize.X;

    public float RegionHeight => RegionSize.Y;

    public Box this[int index] => GetRegion(index);

    public Box this[int col, int row] => GetRegion(col, row);

    public Box this[Vector2 position] => GetRegion(position);

    public int Count => _boxes.Count;

    public TextureAtlas(Texture texture, Vector2 nbRegion, float spacing = 0)
        : this(texture, (int)nbRegion.X, (int)nbRegion.Y, spacing) { }

    public TextureAtlas(Texture texture, int nbCols, int nbRows, float spacing = 0)
        : this(texture, texture.Width / (float)nbCols, texture.Height / (float)nbRows, nbCols * nbRows, spacing) { }

    public TextureAtlas(Texture texture, Vector2 regionSize, int nbRegion, float spacing = 0)
        : this(texture, regionSize.X, regionSize.Y, nbRegion, spacing) { }

    public TextureAtlas(Texture texture, float regionWidth, float regionHeight, int nbRegion, float spacing = 0)
    {
        _boxes = new List<Box>(nbRegion);
        Texture = texture;
        RegionSize = new Vector2(regionWidth, regionHeight);
        Spacing = spacing;
        var offsetX = 0.0f;
        var offsetY = 0.0f;
        for (var i = 0; i < nbRegion; i++)
        {
            var box = new Box(offsetX, offsetY, regionWidth, regionHeight);
            _boxes.Add(box);
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
        var cols = (int)(Width / (RegionWidth + Spacing));
        return row * cols + col;
    }

    public IEnumerator<Box> GetEnumerator()
    {
        return _boxes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

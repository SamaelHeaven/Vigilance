using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Grid : Drawable<Grid>, IFullCloneable
{
    public Grid() { }

    public Grid(Color color)
    {
        Color = color;
    }

    public Grid(float cellSize)
    {
        CellSize = cellSize;
    }

    public Grid(float cellSize, Color color)
        : this(cellSize)
    {
        Color = color;
    }

    public float CellSize { get; set; }
    public float Thick { get; set; } = Drawing.DefaultStrokeWidth == 0 ? 1 : Drawing.DefaultStrokeWidth;
    public Color Color { get; set; } = Drawing.DefaultFill;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawGrid(transform, this);
    }
}

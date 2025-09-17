using Vigilance.Core;
using Vigilance.Logging;

namespace Vigilance.Drawing;

public sealed class Grid : IFullCloneable
{
    public Grid() { }

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
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}

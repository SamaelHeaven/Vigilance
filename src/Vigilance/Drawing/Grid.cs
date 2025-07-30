using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed record Grid : IFullCloneable
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
    public float Thick { get; set; } = 1;
    public Color Color { get; set; } = Color.White;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;
}

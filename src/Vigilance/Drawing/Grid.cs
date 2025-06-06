using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class Grid
{
    public Grid() { }

    public Grid(float cellSize)
    {
        CellSize = cellSize;
    }

    public float CellSize { get; set; }
    public float Thickness { get; set; } = 1;
    public Color Color { get; set; } = Color.Transparent;
    public GetCameraDelegate? Camera { get; set; } = Core.Camera.DefaultDelegate;
}

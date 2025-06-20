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
    public float Thick { get; set; } = 1;
    public Color Color { get; set; } = Color.White;
    public CameraFunc? Camera { get; set; } = Core.Camera.Default;
}

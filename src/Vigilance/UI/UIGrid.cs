using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.UI;

public class UIGrid : UIContainer
{
    private Grid _grid = new();

    public UIGrid() { }

    public UIGrid(Color color)
    {
        Color = color;
    }

    public UIGrid(float cellSize)
    {
        CellSize = cellSize;
    }

    public UIGrid(float cellSize, Color color)
        : this(cellSize)
    {
        Color = color;
    }

    public float CellSize
    {
        get => _grid.CellSize;
        set => _grid.CellSize = value;
    }

    public float Thick
    {
        get => _grid.Thick;
        set => _grid.Thick = value;
    }

    public Color Color
    {
        get => _grid.Color;
        set => _grid.Color = value;
    }

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        _grid.Camera = camera;
        graphics.DrawGrid(LayoutPosition, LayoutSize, _grid);
    }

    protected override void OnClone()
    {
        _grid = _grid.DeepClone();
    }
}

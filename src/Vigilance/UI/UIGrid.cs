using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.UI;

public class UIGrid : UIContainer
{
    private Grid _grid = new();

    public UIGrid() { }

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

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        _grid.Camera = camera;
        graphics.DrawGrid(LayoutPosition, LayoutSize, _grid);
        base.Render(graphics, camera);
    }

    protected override object DeepClone()
    {
        var result = (UIGrid)base.DeepClone();
        result._grid = _grid.DeepClone();
        return result;
    }
}

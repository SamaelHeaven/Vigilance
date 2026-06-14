using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.UI;

public class UIRectangleGradient : UIContainer
{
    private RectangleGradient _rectangle = new();

    public Color TopLeftFill
    {
        get => _rectangle.TopLeftFill;
        set => _rectangle.TopLeftFill = value;
    }

    public Color BottomLeftFill
    {
        get => _rectangle.BottomLeftFill;
        set => _rectangle.BottomLeftFill = value;
    }

    public Color BottomRightFill
    {
        get => _rectangle.BottomRightFill;
        set => _rectangle.BottomRightFill = value;
    }

    public Color TopRightFill
    {
        get => _rectangle.TopRightFill;
        set => _rectangle.TopRightFill = value;
    }

    public Color Stroke
    {
        get => _rectangle.Stroke;
        set => _rectangle.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _rectangle.StrokeWidth;
        set => _rectangle.StrokeWidth = value;
    }

    public DrawOrder DrawOrder
    {
        get => _rectangle.DrawOrder;
        set => _rectangle.DrawOrder = value;
    }

    public Color Fill
    {
        get => _rectangle.Fill;
        set => _rectangle.Fill = value;
    }

    public Color TopFill
    {
        get => _rectangle.TopFill;
        set => _rectangle.TopFill = value;
    }

    public Color BottomFill
    {
        get => _rectangle.BottomFill;
        set => _rectangle.BottomFill = value;
    }

    public Color LeftFill
    {
        get => _rectangle.LeftFill;
        set => _rectangle.LeftFill = value;
    }

    public Color RightFill
    {
        get => _rectangle.RightFill;
        set => _rectangle.RightFill = value;
    }

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        _rectangle.Camera = camera;
        graphics.DrawRectangleGradient(LayoutPosition, LayoutSize, _rectangle);
    }

    protected override void OnClone()
    {
        _rectangle = _rectangle.ShallowClone();
    }
}

using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIRectangle : UIContainer
{
    private Rectangle _rectangle = new();

    public UIRectangle() { }

    public UIRectangle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill
    {
        get => _rectangle.Fill;
        set => _rectangle.Fill = value;
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

    public Unit Radius { get; set; }

    protected override void RenderSelf(Graphics graphics, CameraProvider camera)
    {
        _rectangle.Camera = camera;
        _rectangle.Radius = Radius.Calculate(LayoutSize.X.Abs().Min(LayoutSize.Y.Abs()));
        graphics.DrawRectangle(LayoutPosition, LayoutSize, _rectangle);
    }

    protected override object DeepClone()
    {
        var result = (UIRectangle)base.DeepClone();
        result._rectangle = _rectangle.DeepClone();
        return result;
    }
}

using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UICircle : UIContainer
{
    private Circle _circle = new();

    public UICircle() { }

    public UICircle(Color fill)
    {
        Fill = fill;
    }

    public Color Fill
    {
        get => _circle.Fill;
        set => _circle.Fill = value;
    }

    public Color Stroke
    {
        get => _circle.Stroke;
        set => _circle.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _circle.StrokeWidth;
        set => _circle.StrokeWidth = value;
    }

    public DrawOrder DrawOrder
    {
        get => _circle.DrawOrder;
        set => _circle.DrawOrder = value;
    }

    protected override void RenderSelf(Graphics graphics, CameraProvider camera)
    {
        var position = LayoutPosition;
        var size = LayoutSize;
        _circle.Camera = camera;
        graphics.DrawCircle(new Transform(position + size * 0.5f, size), _circle);
    }

    protected override object DeepClone()
    {
        var result = (UICircle)base.DeepClone();
        result._circle = _circle.DeepClone();
        return result;
    }
}

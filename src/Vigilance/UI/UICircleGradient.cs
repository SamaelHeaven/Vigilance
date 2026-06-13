using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UICircleGradient : UIContainer
{
    private CircleGradient _circle = new();

    public Color InnerFill
    {
        get => _circle.InnerFill;
        set => _circle.InnerFill = value;
    }

    public Color OuterFill
    {
        get => _circle.OuterFill;
        set => _circle.OuterFill = value;
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

    public int Segments
    {
        get => _circle.Segments;
        set => _circle.Segments = value;
    }

    public DrawOrder DrawOrder
    {
        get => _circle.DrawOrder;
        set => _circle.DrawOrder = value;
    }

    public Color Fill
    {
        get => _circle.Fill;
        set => _circle.Fill = value;
    }

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        var position = LayoutPosition;
        var size = LayoutSize;
        _circle.Camera = camera;
        graphics.DrawCircleGradient(new Transform(position + size * 0.5f, size), _circle);
    }

    protected override void OnClone()
    {
        _circle = _circle.ShallowClone();
    }
}

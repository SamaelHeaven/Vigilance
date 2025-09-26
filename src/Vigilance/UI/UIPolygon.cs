using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UIPolygon : UIContainer
{
    private RegularPolygon _polygon = new();

    public UIPolygon() { }

    public UIPolygon(int sides)
    {
        Sides = sides;
    }

    public UIPolygon(int sides, Color fill)
        : this(sides)
    {
        Fill = fill;
    }

    public int Sides
    {
        get => _polygon.Sides;
        set => _polygon.Sides = value;
    }

    public Color Fill
    {
        get => _polygon.Fill;
        set => _polygon.Fill = value;
    }

    public Color Stroke
    {
        get => _polygon.Stroke;
        set => _polygon.Stroke = value;
    }

    public float StrokeWidth
    {
        get => _polygon.StrokeWidth;
        set => _polygon.StrokeWidth = value;
    }

    public DrawOrder DrawOrder
    {
        get => _polygon.DrawOrder;
        set => _polygon.DrawOrder = value;
    }

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        var position = LayoutPosition;
        var size = LayoutSize;
        _polygon.Camera = camera;
        graphics.DrawRegularPolygon(new Transform(position + size * 0.5f, size), _polygon);
        base.Render(graphics, camera);
    }

    protected override object DeepClone()
    {
        var result = (UIPolygon)base.DeepClone();
        result._polygon = _polygon.DeepClone();
        return result;
    }
}

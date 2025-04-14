using Vigilance.Core;

namespace Vigilance.Drawing;

public struct RegularPolygon
{
    public int Sides = 0;
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public RegularPolygon() { }
}

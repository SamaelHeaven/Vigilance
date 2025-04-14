using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Rectangle
{
    public Color Fill = Color.Transparent;
    public Color Stroke = Color.Transparent;
    public float StrokeWidth = 0;
    public float Roundness = 0;
    public CameraProvider? Camera = Core.Camera.DefaultProvider;

    public Rectangle() { }
}

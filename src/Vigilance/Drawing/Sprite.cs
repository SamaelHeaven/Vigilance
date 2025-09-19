using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Sprite : IFullCloneable
{
    public Sprite() { }

    public Sprite(Texture texture)
    {
        Texture = texture;
    }

    public Texture Texture { get; set; } = Drawing.DefaultTexture;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Box? Source { get; set; } = null;
    public Color Tint { get; set; } = Color.White;
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;

    public override string ToString()
    {
        return ObjectPrinter.Print(this);
    }
}

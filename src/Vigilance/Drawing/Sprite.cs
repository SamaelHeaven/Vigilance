using Vigilance.Core;
using Vigilance.Logging;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class Sprite : Drawable<Sprite>, IFullCloneable
{
    public Sprite() { }

    public Sprite(Texture texture)
    {
        Texture = texture;
        Scale = texture.Size;
    }

    public Texture Texture { get; set; } = Drawing.DefaultTexture;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    public Box? Source { get; set; } = null;
    public Color Tint { get; set; } = Color.White;
    public NPatchInfo? NPatchInfo { get; set; } = null;
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;

    public override string ToString()
    {
        return ObjectPrinter.Print(this, ObjectPrinter.Exclude(nameof(Transform)), true);
    }

    public override void Render(Transform transform, Graphics graphics)
    {
        graphics.DrawSprite(transform, this);
    }
}

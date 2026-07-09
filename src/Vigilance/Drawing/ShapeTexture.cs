using Vigilance.Math;

namespace Vigilance.Drawing;

public readonly struct ShapeTexture
{
    public ShapeTexture(Texture texture)
        : this(texture, null) { }

    public ShapeTexture(Texture texture, in Box? source)
    {
        Texture = texture;
        Source = source ?? new Box(Vector2.Zero, texture.Size);
    }

    public Texture Texture { get; }
    public Box Source { get; }

    public static implicit operator ShapeTexture(Texture texture)
    {
        return new ShapeTexture(texture);
    }
}

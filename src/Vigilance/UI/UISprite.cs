using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.UI;

public class UISprite : UIContainer
{
    private Sprite _sprite = new();

    public UISprite() { }

    public UISprite(Texture texture)
    {
        Texture = texture;
    }

    public Texture Texture
    {
        get => _sprite.Texture;
        set => _sprite.Texture = value;
    }

    public bool FlipX
    {
        get => _sprite.FlipX;
        set => _sprite.FlipX = value;
    }

    public bool FlipY
    {
        get => _sprite.FlipY;
        set => _sprite.FlipY = value;
    }

    public Box? Source
    {
        get => _sprite.Source;
        set => _sprite.Source = value;
    }

    public Color Tint
    {
        get => _sprite.Tint;
        set => _sprite.Tint = value;
    }

    public Interpolation Interpolation
    {
        get => _sprite.Interpolation;
        set => _sprite.Interpolation = value;
    }

    public override object DeepClone()
    {
        var result = (UISprite)base.DeepClone();
        result._sprite = _sprite.DeepClone();
        return result;
    }

    public override void Render(Graphics graphics, CameraFunc? camera)
    {
        _sprite.Camera = camera;
        graphics.DrawSprite(LayoutPosition, LayoutSize, _sprite);
        base.Render(graphics, camera);
    }
}

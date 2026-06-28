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
        Size = texture.Size;
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

    public NPatchInfo? NPatchInfo
    {
        get => _sprite.NPatchInfo;
        set => _sprite.NPatchInfo = value;
    }

    public TextureFilter TextureFilter
    {
        get => _sprite.TextureFilter;
        set => _sprite.TextureFilter = value;
    }

    public TextureWrap TextureWrap
    {
        get => _sprite.TextureWrap;
        set => _sprite.TextureWrap = value;
    }

    protected override void OnRender(Graphics graphics, CameraProvider camera)
    {
        _sprite.Camera = camera;
        graphics.DrawSprite(LayoutPosition, LayoutSize, _sprite);
    }

    protected override void OnClone()
    {
        _sprite = _sprite.DeepClone();
    }
}

public static class UISpriteExtensions
{
    extension(SpriteAnimationFrame frame)
    {
        public void UpdateUISprite(UISprite sprite)
        {
            if (frame.Texture is not null)
                sprite.Texture = frame.Texture;
            if (frame.FlipX.HasValue)
                sprite.FlipX = frame.FlipX.Value;
            if (frame.FlipY.HasValue)
                sprite.FlipY = frame.FlipY.Value;
            if (frame.Source.HasValue)
                sprite.Source = frame.Source;
            if (frame.Tint.HasValue)
                sprite.Tint = frame.Tint.Value;
            if (frame.NPatchInfo.HasValue)
                sprite.NPatchInfo = frame.NPatchInfo;
            if (frame.TextureFilter.HasValue)
                sprite.TextureFilter = frame.TextureFilter.Value;
            if (frame.Position.HasValue)
                sprite.Translate = frame.Position.Value;
            if (frame.Scale.HasValue)
                sprite.Scale = frame.Scale.Value;
            if (frame.Rotation.HasValue)
                sprite.Rotation = frame.Rotation.Value;
            if (frame.PivotPoint.HasValue)
                sprite.PivotPoint = frame.PivotPoint.Value;
            if (frame.BlendMode.HasValue)
                sprite.BlendMode = frame.BlendMode.Value;
            if (frame.Shader.HasValue)
                sprite.Shader = frame.Shader;
            if (frame.Culling.HasValue)
                sprite.Culling = frame.Culling.Value;
        }
    }
}

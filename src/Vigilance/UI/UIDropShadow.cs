using Vigilance.Core;
using Vigilance.Drawing;
using Color = Vigilance.Drawing.Color;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public class UIDropShadow : UIElement
{
    private int _blur;

    public UIDropShadow(UIElement target, int blur = 1, Color? color = null)
    {
        Target = target;
        Color = color ?? Color.Black;
        Blur = blur;
    }

    public UIElement Target { get; }
    public Color Color { get; set; }
    public bool TextureDirty { get; private set; } = true;
    public Texture Texture { get; private set; } = Texture.Empty;

    public int Blur
    {
        get => _blur;
        set
        {
            if (_blur == value)
                return;
            _blur = value;
            MarkTextureDirty();
        }
    }

    public void MarkTextureDirty()
    {
        TextureDirty = true;
    }

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        var offset = 1 + _blur * _blur;
        if (TextureDirty || Dirty || Target.Dirty)
        {
            TextureDirty = false;
            var image = Target.ToTexture(Target.LayoutSize).ToImage();
            var result = new WritableImage<PixelGrayAlpha>(image.Width + offset * 2, image.Height + offset * 2);
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                result[x + offset, y + offset] = new PixelGrayAlpha(255, pixel.A);
            }

            result.Blur(_blur);
            Texture = result.ToTexture();
        }

        graphics.DrawTexture(Texture, LayoutPosition - offset, null, Color, camera: camera);
    }

    protected override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return Target.LayoutSize;
    }
}

public static class UIDropShadowExtensions
{
    public static UIContainer DropShadow(
        this UIElement element,
        int blur = 1,
        Color? color = null,
        Dimensions? translate = null
    )
    {
        var result = new UIContainer();
        result.Add(
            new UIDropShadow(element, blur, color) { Position = Position.Absolute, Translate = translate ?? default }
        );
        result.Add(element);
        return result;
    }
}

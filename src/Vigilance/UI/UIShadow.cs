using Vigilance.Core;
using Vigilance.Drawing;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public class UIShadow : UIElement
{
    private int _blur;
    private bool _dirty = true;
    private Texture _texture = null!;

    public UIShadow(UIElement target, Color? color = null, int blur = 1, Interpolation? interpolation = null)
    {
        Target = target;
        Color = color ?? Color.Black;
        Blur = blur;
        Interpolation = interpolation ?? Drawing.Drawing.DefaultInterpolation;
    }

    public UIElement Target { get; }

    public Color Color { get; set; }

    public Interpolation Interpolation { get; set; }

    public int Blur
    {
        get => _blur;
        set
        {
            if (_blur == value)
                return;
            _blur = value;
            MarkDirty();
        }
    }

    public new void MarkDirty()
    {
        _dirty = true;
    }

    public override void Update(Entity entity)
    {
        base.Update(entity);
        if (Target.Dirty)
            base.MarkDirty();
    }

    protected override void Render(Graphics graphics, CameraProvider camera)
    {
        var offset = _blur * 2;
        if (_dirty)
        {
            _dirty = false;
            var image = Target.ToTexture(Target.Parent!.LayoutSize).ToImage();
            var result = new WritableImage<PixelGrayAlpha>(image.Width + offset * 2, image.Height + offset * 2);
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                result[x + offset, y + offset] = new PixelGrayAlpha(255, pixel.A);
            }

            result.Blur(_blur);
            _texture = result.ToTexture();
        }

        graphics.DrawTexture(_texture, LayoutPosition - offset, null, Color, Interpolation, camera);
    }

    public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        MarkDirty();
        return Target.Measure(width, widthMode, height, heightMode);
    }
}

public static class UIShadowExtensions
{
    public static UIContainer Shadow(
        this UIElement element,
        Color? color = null,
        int blur = 1,
        Interpolation? interpolation = null,
        Dimensions? translate = null
    )
    {
        var result = new UIContainer();
        result.Add(
            new UIShadow(element, color, blur, interpolation)
            {
                Position = Position.Absolute,
                Translate = translate ?? default,
            }
        );
        result.Add(element);
        return result;
    }
}

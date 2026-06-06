using Vigilance.Core;
using Vigilance.Drawing;
using Color = Vigilance.Drawing.Color;

namespace Vigilance.UI;

public class UIDropShadow : IUIComponent
{
    private readonly Func<UIElement, Graphics, CameraProvider, bool> _onBeginRenderHandler;
    private readonly Func<UIElement, bool> _onDirtyHandler;
    private int _blur;
    private bool _isTextureUsed;
    private Texture _texture = Texture.Empty;

    public UIDropShadow(int blur = 1, Color? color = null)
    {
        Color = color ?? Color.Black;
        Blur = blur;
        _onBeginRenderHandler = BeginRender;
        _onDirtyHandler = _ =>
        {
            MarkTextureDirty();
            return false;
        };
    }

    public Color Color { get; set; }
    public bool IsTextureDirty { get; private set; } = true;

    public Texture Texture
    {
        get
        {
            _isTextureUsed = true;
            return _texture;
        }
    }

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

    public void Attach(UIElement element)
    {
        element.OnDirtySignal.Subscribe(_onDirtyHandler);
        element.OnBeginRenderSignal.Subscribe(_onBeginRenderHandler);
    }

    public void Detach(UIElement element)
    {
        element.OnDirtySignal.Unsubscribe(_onDirtyHandler);
        element.OnBeginRenderSignal.Unsubscribe(_onBeginRenderHandler);
    }

    public void MarkTextureDirty()
    {
        IsTextureDirty = true;
    }

    private bool BeginRender(UIElement element, Graphics graphics, CameraProvider camera)
    {
        var offset = 1 + _blur * _blur;
        if (IsTextureDirty)
        {
            IsTextureDirty = false;
            using var targetTexture = element
                .ShallowClone()
                .ToTexture(element.Parent?.LayoutSize ?? element.LayoutSize);
            var image = targetTexture.ToImage();
            var result = new WritableImage<PixelGrayAlpha>(image.Width + offset * 2, image.Height + offset * 2);
            for (var y = 0; y < image.Height; y++)
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                result[x + offset, y + offset] = new PixelGrayAlpha(255, pixel.A);
            }

            result.Blur(_blur);
            if (!_isTextureUsed)
                _texture.Dispose();
            _texture = result.ToTexture();
        }

        var previousClip = graphics.SetClip(null);
        graphics.DrawTexture(_texture, element.LayoutPosition - offset, null, Color, camera: camera);
        graphics.SetClip(previousClip);
        return false;
    }
}

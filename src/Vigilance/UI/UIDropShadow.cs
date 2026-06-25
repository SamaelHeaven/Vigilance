using Vigilance.Core;
using Vigilance.Drawing;
using Color = Vigilance.Drawing.Color;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.UI;

public sealed class UIDropShadow : IUIComponent, IFullCloneable
{
    private readonly Shader _blurShader = Shader.Fragment.Resource(
        "Resources.Shader.blur.frag.glsl",
        "",
        Assemblies.Engine
    );

    private readonly Shader _silhouetteShader = Shader.Fragment.Resource(
        "Resources.Shader.silhouette.frag.glsl",
        "",
        Assemblies.Engine
    );

    private int _blur;
    private bool _isTextureUsed;
    private Func<UIElement, Graphics, CameraProvider, bool> _onBeginRenderHandler = null!;
    private Func<UIElement, bool> _onDirtyHandler = null!;
    private RenderTexture? _renderTexture;
    private Texture _texture = null!;

    public UIDropShadow(int blur = 1, Color? color = null)
    {
        Color = color ?? Color.Black;
        Blur = blur;
        Initialize();
    }

    public Color Color { get; set; }
    public bool IsTextureDirty { get; private set; }
    public BlendMode BlendMode { get; set; } = Drawing.Drawing.DefaultBlendMode;
    public Shader Shader { get; set; } = Drawing.Drawing.DefaultShader;

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

    object IDeepCloneable.DeepClone()
    {
        return this.ShallowClone();
    }

    object IShallowCloneable.ShallowClone()
    {
        var clone = (UIDropShadow)Cloner.MemberwiseClone(this);
        clone.Initialize();
        return clone;
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

    private void Initialize()
    {
        IsTextureDirty = true;
        _renderTexture = null;
        _texture = Texture.Empty;
        _isTextureUsed = false;
        _onBeginRenderHandler = BeginRender;
        _onDirtyHandler = _ =>
        {
            MarkTextureDirty();
            return false;
        };
    }

    private bool BeginRender(UIElement element, Graphics graphics, CameraProvider camera)
    {
        var offset = 1 + _blur * 3;
        if (IsTextureDirty)
        {
            IsTextureDirty = false;
            RebuildTexture(element, offset);
        }

        var previousClip = graphics.SetClip(null);
        var previousBlendMode = graphics.SetBlendMode(BlendMode);
        var previousShader = graphics.SetShader(Shader);
        graphics.DrawTexture(_texture, element.LayoutPosition - offset, null, Color, camera: camera);
        graphics.SetClip(previousClip);
        graphics.SetBlendMode(previousBlendMode);
        graphics.SetShader(previousShader);
        return false;
    }

    private void RebuildTexture(UIElement element, int offset)
    {
        var clone = element.ShallowClone();
        clone.ResetLayoutAndTransform();
        using var elementTexture = clone.ToTexture(element.Parent?.LayoutSize ?? element.LayoutSize);
        var width = elementTexture.ScaledWidth + offset * 2;
        var height = elementTexture.ScaledHeight + offset * 2;
        var silhouette = new RenderTexture(width, height);
        silhouette.Graphics.SetBlendMode(BlendMode.Replace);
        silhouette.Graphics.SetShader(_silhouetteShader);
        silhouette.Graphics.DrawTexture(
            elementTexture,
            new Vector2(offset, offset),
            interpolation: Interpolation.Nearest
        );
        RenderTexture result;
        if (_blur > 0)
        {
            var sigma = (float)_blur;
            var radius = int.Min(_blur * 3, offset);
            var horizontal = RenderBlurPass(silhouette, new Vector2(1, 0), radius, sigma);
            result = RenderBlurPass(horizontal, new Vector2(0, 1), radius, sigma);
            Graphics.DrawCurrentBuffer();
            silhouette.Dispose();
            horizontal.Dispose();
        }
        else
        {
            result = silhouette;
        }

        if (!_isTextureUsed)
            _renderTexture?.Dispose();
        _renderTexture = result;
        _texture = result.Texture;
        _isTextureUsed = false;
    }

    private RenderTexture RenderBlurPass(RenderTexture source, Vector2 direction, int radius, float sigma)
    {
        var target = new RenderTexture(source.ScaledWidth, source.ScaledHeight);
        _blurShader.SetVec2("direction", direction);
        _blurShader.SetInt("radius", radius);
        _blurShader.SetFloat("sigma", sigma);
        target.Graphics.SetBlendMode(BlendMode.Replace);
        target.Graphics.SetShader(_blurShader);
        target.Graphics.DrawTexture(source, Vector2.Zero, interpolation: Interpolation.Nearest);
        return target;
    }
}

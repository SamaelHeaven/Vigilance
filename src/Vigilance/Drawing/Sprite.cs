using Vigilance.Core;

namespace Vigilance.Drawing;

public struct Sprite
{
    private Texture _currentTexture = null!;
    private Texture _texture = Texture.Empty;
    private int _blur = 0;
    private bool _flippedHorizontally = false;
    private bool _flippedVertically = false;
    private bool _clean = false;

    internal Texture CurrentTexture
    {
        get
        {
            if (_texture.Writable)
                _clean = false;
            if (_clean)
                return _currentTexture;
            if (_blur <= 0 && !_flippedHorizontally && !_flippedVertically)
            {
                _currentTexture = _texture;
                _clean = true;
                return _currentTexture;
            }

            var image = _texture.ToImage();
            if (_blur > 0)
                image.Blur(_blur);
            if (_flippedHorizontally)
                image.FlipHorizontally();
            if (_flippedVertically)
                image.FlipVertically();
            _currentTexture = image.ToTexture();
            _clean = true;
            return _currentTexture;
        }
    }

    public Texture Texture
    {
        get => _texture;
        set
        {
            if (_texture == value)
                return;
            _texture = value;
            _clean = false;
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
            _clean = false;
        }
    }

    public bool FlippedHorizontally
    {
        get => _flippedHorizontally;
        set
        {
            if (_flippedHorizontally == value)
                return;
            _flippedHorizontally = value;
            _clean = false;
        }
    }

    public bool FlippedVertically
    {
        get => _flippedVertically;
        set
        {
            if (_flippedVertically == value)
                return;
            _flippedVertically = value;
            _clean = false;
        }
    }

    public Interpolation Interpolation = Game.DefaultInterpolation;
    public Color Tint = Color.White;
    public Func<Camera>? Camera = () => Game.Scene.Camera;

    public Sprite() { }
}

namespace Vigilance.Drawing;

public struct Drawable<TSelf>
{
    public CameraProvider Camera { get; set; } = Drawing.DefaultCamera;
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public BlendMode? BlendMode { get; set; } = null;
    public Shader? Shader { get; set; } = null;
    public ShapeTexture? ShapeTexture { get; set; } = null;
    public bool? Culling { get; set; } = null;
    public Action<Transform, TSelf, Graphics>? OnBeginDrawing { get; set; } = null;
    public Action<Transform, TSelf, Graphics>? OnEndDrawing { get; set; } = null;

    public Drawable() { }

    public Transform Transform
    {
        readonly get => new(Position, Scale, Rotation, PivotPoint);
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public static DrawScope EnterDrawing(
        scoped ref Transform transform,
        in Drawable<TSelf> drawable,
        in TSelf self,
        Graphics graphics
    )
    {
        var originalTransform = transform;
        drawable.OnBeginDrawing?.Invoke(originalTransform, self, graphics);
        BlendMode? previousBlendMode = null;
        Shader? previousShader = null;
        ShapeTexture? previousShapeTexture = null;
        bool? previousCulling = null;
        if (drawable.BlendMode.HasValue)
            previousBlendMode = graphics.SetBlendMode(drawable.BlendMode.Value);
        if (drawable.Shader is not null)
            previousShader = graphics.SetShader(drawable.Shader);
        if (drawable.ShapeTexture is not null)
            previousShapeTexture = graphics.SetShapeTexture(drawable.ShapeTexture);
        if (drawable.Culling.HasValue)
            previousCulling = graphics.SetCulling(drawable.Culling.Value);
        transform += drawable.Transform;
        graphics.PushMatrix();
        return new DrawScope(
            originalTransform,
            in self,
            in drawable,
            graphics,
            previousBlendMode,
            previousShader,
            previousShapeTexture,
            previousCulling
        );
    }

    public readonly ref struct DrawScope(
        scoped in Transform transform,
        ref readonly TSelf self,
        ref readonly Drawable<TSelf> drawable,
        Graphics graphics,
        BlendMode? previousBlendMode = null,
        Shader? previousShader = null,
        scoped in ShapeTexture? previousShapeTexture = null,
        bool? previousCulling = null
    ) : IDisposable
    {
        private readonly Transform _transform = transform;
        private readonly ref readonly TSelf _self = ref self;
        private readonly ref readonly Drawable<TSelf> _drawable = ref drawable;
        private readonly Graphics _graphics = graphics;
        private readonly BlendMode? _previousBlendMode = previousBlendMode;
        private readonly Shader? _previousShader = previousShader;
        private readonly ShapeTexture? _previousShapeTexture = previousShapeTexture;
        private readonly bool? _previousCulling = previousCulling;

        public void Dispose()
        {
            _graphics.PopMatrix();
            if (_previousBlendMode.HasValue)
                _graphics.SetBlendMode(_previousBlendMode.Value);
            if (_previousShader is not null)
                _graphics.SetShader(_previousShader);
            if (_drawable.ShapeTexture is not null)
                _graphics.SetShapeTexture(_previousShapeTexture);
            if (_previousCulling.HasValue)
                _graphics.SetCulling(_previousCulling.Value);
            _drawable.OnEndDrawing?.Invoke(_transform, _self, _graphics);
        }
    }
}

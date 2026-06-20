using System.Runtime.InteropServices;
using Raylib_cs;
using Vigilance.Math;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SpriteInstance : ISpriteInstance<SpriteInstance>
{
    public const string VertexShader = """

            #version vigilance_300

            layout(location = 0) in vec2 vertexPosition;
            layout(location = 1) in vec2 vertexTexCoord;
            layout(location = 2) in vec2 instancePosition;
            layout(location = 3) in vec2 instanceScale;
            layout(location = 4) in float instanceRotation;
            layout(location = 5) in vec2 instancePivotPoint;
            layout(location = 6) in vec4 instanceTint;
            layout(location = 7) in int instanceFlipX;
            layout(location = 8) in int instanceFlipY;
            layout(location = 9) in int instanceHasSource;
            layout(location = 10) in vec4 instanceSource;

            out vec2 fragTexCoord;
            out vec4 fragColor;

            uniform mat4 mvp; 
            uniform vec2 transformPosition;
            uniform vec2 transformScale;
            uniform float transformRotation;
            uniform vec2 transformPivotPoint;
            uniform vec2 textureSize;
            uniform int flipY; 

            vec2 rotate(vec2 v, float deg)
            {
                float rad = radians(deg);
                float s = sin(rad);
                float c = cos(rad);
                return vec2(v.x * c - v.y * s, v.x * s + v.y * c);
            }

            void main()
            {
                vec4 source = instanceHasSource == 0 ? vec4(0, 0, textureSize) : instanceSource;
                vec2 invTextureSize = 1.0 / textureSize;
                float sw = instanceFlipX == 0 ? source.z : -source.z;
                float sh = instanceFlipY == 0 ? source.w : -source.w;
                float sx = source.x;
                float sy = source.y < 0 ? source.y - sh : source.y;
                bool flipX = sw < 0.0;
                sw = abs(sw);
                float uLeft = sx * invTextureSize.x;
                float uRight = (sx + sw) * invTextureSize.x;
                float vTop = sy * invTextureSize.y;
                float vBottom = (sy + sh) * invTextureSize.y;
                float tx = flipX ? (1.0 - vertexTexCoord.x) : vertexTexCoord.x;
                float u = mix(uLeft, uRight, tx);
                float v = mix(vTop, vBottom, vertexTexCoord.y);
                v = flipY == 0 ? v : 1.0 - v;
                fragTexCoord = vec2(u, v);
                fragColor = instanceTint;
                vec2 position = instancePosition + transformPosition;
                vec2 scale = abs(instanceScale) * transformScale;
                float rotation = instanceRotation + transformRotation;
                vec2 pivotPoint = instancePivotPoint + transformPivotPoint;
                vec2 rotated = rotate(vertexPosition * scale - pivotPoint, rotation) + pivotPoint;
                gl_Position = vec4(rotated + position, 0.0, 1.0) * mvp;
            }
            
        """;

    public const string FragmentShader = """

            #version vigilance_300

            in vec2 fragTexCoord;
            in vec4 fragColor;
            
            out vec4 finalColor;

            uniform sampler2D texture0;

            void main()
            {
                vec4 texelColor = texture(texture0, fragTexCoord);
                finalColor = texelColor * fragColor;
            }
            
        """;

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; } = 0;
    public Vector2 PivotPoint { get; set; } = Vector2.Zero;
    public Color Tint { get; set; } = Color.White;
    public bool FlipX { get; set; } = false;
    public bool FlipY { get; set; } = false;
    private bool _hasSource = false;
    private Box _source = default;

    public Box? Source
    {
        get => _hasSource ? _source : null;
        set
        {
            _hasSource = value.HasValue;
            _source = value ?? default;
        }
    }

    public SpriteInstance() { }

    public Transform Transform
    {
        get => new(Position, Scale, Rotation, PivotPoint);
        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            PivotPoint = value.PivotPoint;
        }
    }

    public static Shader Shader => field ??= new Shader(VertexShader, FragmentShader);

    public static void Draw(SpriteBatch<SpriteInstance> batch, Transform transform, Graphics graphics)
    {
        if (batch.Count == 0)
            return;
        using var _ = Drawable.EnterDrawing(ref transform, batch, graphics);
        batch.Sync();
        graphics.BeginDrawing(batch.Camera);
        var texture = batch.Texture;
        var shader = batch.Shader;
        texture.Interpolation = batch.Interpolation;
        texture.TextureWrap = batch.TextureWrap;
        if (shader is not null)
        {
            shader.SetMatrix("mvp", Graphics.GetCurrentMatrix());
            shader.SetInt("texture0", 0);
            shader.SetVec2("transformPosition", transform.Position);
            shader.SetVec2("transformScale", transform.Scale.Abs());
            shader.SetFloat("transformRotation", transform.Rotation);
            shader.SetVec2("transformPivotPoint", transform.PivotPoint);
            shader.SetVec2("textureSize", texture.Size);
            shader.SetInt("flipY", texture.IsRenderTexture ? 1 : 0);
        }

        var index = 0u;
        var offset = 0;
        var size = sizeof(float) * 4;
        Rlgl.ActiveTextureSlot(0);
        Rlgl.EnableTexture(texture.Id);
        Rlgl.EnableVertexArray(batch.VertexArrayId);
        Rlgl.EnableVertexBuffer(batch.VertexBufferId);
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        index++;
        offset += sizeof(float) * 2;
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        index++;
        offset = 0;
        size = sizeof(SpriteInstance);
        Rlgl.EnableVertexBuffer(batch.InstanceBufferId);
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(Vector2);
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(Vector2);
        Rlgl.SetVertexAttribute(index, 1, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(float);
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(Vector2);
        Rlgl.SetVertexAttribute(index, 4, Rlgl.UNSIGNED_BYTE, true, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(Color);
        Rlgl.SetVertexAttribute(index, 1, Rlgl.UNSIGNED_BYTE, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(bool);
        Rlgl.SetVertexAttribute(index, 1, Rlgl.UNSIGNED_BYTE, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(bool);
        Rlgl.SetVertexAttribute(index, 1, Rlgl.UNSIGNED_BYTE, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        index++;
        offset += sizeof(bool) * 2;
        Rlgl.SetVertexAttribute(index, 4, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.SetVertexAttributeDivisor(index, 1);
        Rlgl.DrawVertexArrayInstanced(0, 6, batch.Count);
        Rlgl.DisableVertexArray();
        Rlgl.DisableVertexBuffer();
        Rlgl.DisableTexture();
        graphics.EndDrawing();
    }
}

public static class SpriteInstanceExtensions
{
    extension(SpriteAnimationFrame frame)
    {
        public void UpdateSpriteInstance(ref SpriteInstance sprite)
        {
            if (frame.FlipX.HasValue)
                sprite.FlipX = frame.FlipX.Value;
            if (frame.FlipY.HasValue)
                sprite.FlipY = frame.FlipY.Value;
            if (frame.Source.HasValue)
                sprite.Source = frame.Source;
            if (frame.Tint.HasValue)
                sprite.Tint = frame.Tint.Value;
            if (frame.Position.HasValue)
                sprite.Position = frame.Position.Value;
            if (frame.Scale.HasValue)
                sprite.Scale = frame.Scale.Value;
            if (frame.Rotation.HasValue)
                sprite.Rotation = frame.Rotation.Value;
            if (frame.PivotPoint.HasValue)
                sprite.PivotPoint = frame.PivotPoint.Value;
        }
    }
}

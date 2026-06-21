using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Math;
using ZLinq;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed unsafe class SpriteBatch : SpriteBatch<SpriteInstance>
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
            layout(location = 7) in float instanceFlipX;
            layout(location = 8) in float instanceFlipY;
            layout(location = 9) in float instanceHasSource;
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
                vec4 source = instanceHasSource > 0.5 ? instanceSource : vec4(0, 0, textureSize);
                vec2 invTextureSize = 1.0 / textureSize;
                float sw = instanceFlipX > 0.5 ? -source.z : source.z;
                float sh = instanceFlipY > 0.5 ? -source.w : source.w;
                float sx = source.x;
                float sy = source.y < 0.0 ? source.y - sh : source.y;
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

    private readonly VertexBuffer<float> _quadBuffer = VertexBuffer.Quad();

    private readonly VertexArray _vertexArray = new();

    private int _configuredInstanceBufferVersion = -1;

    public SpriteBatch(Texture texture, Shader? shader = null)
        : base(texture, shader ?? DefaultShader)
    {
        var index = 0u;
        var offset = 0;
        const int size = sizeof(float) * 4;
        Rlgl.EnableVertexArray(_vertexArray.Id);
        Rlgl.EnableVertexBuffer(_quadBuffer.Id);
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        index++;
        offset += sizeof(float) * 2;
        Rlgl.SetVertexAttribute(index, 2, Rlgl.FLOAT, false, size, offset);
        Rlgl.EnableVertexAttribute(index);
        Rlgl.DisableVertexArray();
        Rlgl.DisableVertexBuffer();
    }

    public static Shader DefaultShader => field ??= new Shader(VertexShader, FragmentShader);

    private void ConfigureInstanceAttributes()
    {
        var index = 2u;
        var offset = 0;
        var size = sizeof(SpriteInstance);
        Rlgl.EnableVertexArray(_vertexArray.Id);
        Rlgl.EnableVertexBuffer(InstanceBuffer.Id);
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
        Rlgl.DisableVertexArray();
        Rlgl.DisableVertexBuffer();
        _configuredInstanceBufferVersion = InstanceBuffer.Version;
    }

    public override void Draw(Transform transform, Graphics graphics)
    {
        if (Count == 0)
            return;
        using var _ = EnterDrawing<SpriteBatch<SpriteInstance>>(ref transform, this, graphics);
        InstanceBuffer.Sync();
        if (_configuredInstanceBufferVersion != InstanceBuffer.Version)
            ConfigureInstanceAttributes();
        graphics.BeginDrawing(Camera);
        Texture.Interpolation = Interpolation;
        Texture.TextureWrap = TextureWrap;
        if (Shader is not null)
        {
            Shader.SetMatrix("mvp", Graphics.GetCurrentMatrix());
            Shader.SetInt("texture0", 0);
            Shader.SetVec2("transformPosition", transform.Position);
            Shader.SetVec2("transformScale", transform.Scale.Abs());
            Shader.SetFloat("transformRotation", transform.Rotation);
            Shader.SetVec2("transformPivotPoint", transform.PivotPoint);
            Shader.SetVec2("textureSize", Texture.Size);
            Shader.SetInt("flipY", Texture.IsRenderTexture ? 1 : 0);
        }

        Rlgl.ActiveTextureSlot(0);
        Rlgl.EnableTexture(Texture.Id);
        Rlgl.EnableVertexArray(_vertexArray.Id);
        Rlgl.EnableVertexBuffer(InstanceBuffer.Id);
        Rlgl.DrawVertexArrayInstanced(0, 6, Count);
        Rlgl.DisableVertexArray();
        Rlgl.DisableVertexBuffer();
        Rlgl.DisableTexture();
        graphics.EndDrawing();
    }
}

public abstract class SpriteBatch<TInstance>
    : Drawable<SpriteBatch<TInstance>>,
        IList<TInstance>,
        IValueListView<TInstance>
    where TInstance : unmanaged
{
    protected readonly VertexBuffer<TInstance> InstanceBuffer = [];

    protected SpriteBatch(Texture texture, Shader shader)
    {
        Texture = texture;
        Shader = shader;
    }

    public Texture Texture { get; set; }
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public TextureWrap TextureWrap { get; set; } = Drawing.DefaultTextureWrap;

    public int Count => InstanceBuffer.Count;
    bool ICollection<TInstance>.IsReadOnly => false;

    public TInstance this[int index]
    {
        get => InstanceBuffer[index];
        set => InstanceBuffer[index] = value;
    }

    void ICollection<TInstance>.Add(TInstance item)
    {
        InstanceBuffer.Add(item);
    }

    public void Clear()
    {
        InstanceBuffer.Clear();
    }

    bool ICollection<TInstance>.Contains(TInstance item)
    {
        return InstanceBuffer.Contains(item);
    }

    public void CopyTo(TInstance[] array, int arrayIndex)
    {
        InstanceBuffer.CopyTo(array, arrayIndex);
    }

    bool ICollection<TInstance>.Remove(TInstance item)
    {
        return InstanceBuffer.Remove(item);
    }

    int IList<TInstance>.IndexOf(TInstance item)
    {
        return InstanceBuffer.IndexOf(item);
    }

    void IList<TInstance>.Insert(int index, TInstance item)
    {
        InstanceBuffer.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        InstanceBuffer.RemoveAt(index);
    }

    public ReadOnlySpan<TInstance> AsSpan()
    {
        return InstanceBuffer.AsSpan();
    }

    public ValueList<TInstance>.Enumerator GetEnumerator()
    {
        return InstanceBuffer.GetEnumerator();
    }

    public ValueEnumerable<ValueList<TInstance>.Enumerator, TInstance> AsValueEnumerable()
    {
        return InstanceBuffer.AsValueEnumerable();
    }

    public void Add(in TInstance item)
    {
        InstanceBuffer.Add(item);
    }

    public bool Contains(in TInstance item)
    {
        return InstanceBuffer.Contains(item);
    }

    public bool Remove(in TInstance item)
    {
        return InstanceBuffer.Remove(item);
    }

    public int IndexOf(in TInstance item)
    {
        return InstanceBuffer.IndexOf(item);
    }

    public void Insert(int index, in TInstance item)
    {
        InstanceBuffer.Insert(index, item);
    }

    public abstract override void Draw(Transform transform, Graphics graphics);
}

public static class SpriteBatchExtensions
{
    extension(Graphics graphics)
    {
        public void DrawSpriteBatch<T>(SpriteBatch<T> spriteBatch)
            where T : unmanaged
        {
            spriteBatch.Draw(new Transform(), graphics);
        }

        public void DrawSpriteBatch<T>(in Transform transform, SpriteBatch<T> spriteBatch)
            where T : unmanaged
        {
            spriteBatch.Draw(transform, graphics);
        }
    }
}

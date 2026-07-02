using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using ZLinq;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed unsafe class SpriteBatch : SpriteBatch<SpriteInstance>
{
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

    private static Shader DefaultShader =>
        Shader.Resource(
            ("Shader.sprite-batch.vert.glsl", Assemblies.Engine),
            ("Shader.sprite-batch.frag.glsl", Assemblies.Engine)
        );

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
        Texture.TextureFilter = TextureFilter;
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
    public TextureFilter TextureFilter { get; set; } = Drawing.DefaultTextureFilter;
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

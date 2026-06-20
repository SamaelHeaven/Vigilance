using Vigilance.Collections;
using ZLinq;
using Transform = Vigilance.Math.Transform;

namespace Vigilance.Drawing;

public sealed class SpriteBatch : SpriteBatch<SpriteInstance>
{
    public SpriteBatch(Texture texture)
        : base(texture) { }
}

public class SpriteBatch<TInstance> : Drawable<SpriteBatch<TInstance>>, IValueListView<TInstance>
    where TInstance : unmanaged, ISpriteInstance<TInstance>
{
    private readonly VertexBuffer<TInstance> _instanceBuffer = [];
    private readonly VertexArray _vertexArray = new();

    private readonly VertexBuffer<float> _vertexBuffer =
    [
        -0.5f,
        -0.5f,
        0f,
        0f,
        0.5f,
        0.5f,
        1f,
        1f,
        0.5f,
        -0.5f,
        1f,
        0f,
        -0.5f,
        -0.5f,
        0f,
        0f,
        -0.5f,
        0.5f,
        0f,
        1f,
        0.5f,
        0.5f,
        1f,
        1f,
    ];

    public SpriteBatch(Texture texture)
    {
        Texture = texture;
        Shader = TInstance.Shader;
    }

    public uint InstanceBufferId => _instanceBuffer.Id;
    public uint VertexBufferId => _vertexBuffer.Id;
    public uint VertexArrayId => _vertexArray.Id;

    public Texture Texture { get; set; }
    public Interpolation Interpolation { get; set; } = Drawing.DefaultInterpolation;
    public TextureWrap TextureWrap { get; set; } = Drawing.DefaultTextureWrap;

    public int Count => _instanceBuffer.Count;

    public TInstance this[int index]
    {
        get => _instanceBuffer[index];
        set => _instanceBuffer[index] = value;
    }

    public ReadOnlySpan<TInstance> AsSpan()
    {
        return _instanceBuffer.AsSpan();
    }

    public ValueList<TInstance>.Enumerator GetEnumerator()
    {
        return _instanceBuffer.GetEnumerator();
    }

    public ValueEnumerable<ValueList<TInstance>.Enumerator, TInstance> AsValueEnumerable()
    {
        return _instanceBuffer.AsValueEnumerable();
    }

    public void Add(in TInstance transform)
    {
        _instanceBuffer.Add(transform);
    }

    public void Clear()
    {
        _instanceBuffer.Clear();
    }

    public void RemoveAt(int index)
    {
        _instanceBuffer.RemoveAt(index);
    }

    public void Sync()
    {
        _instanceBuffer.Sync();
        _vertexBuffer.Sync();
    }

    public override void Draw(Transform transform, Graphics graphics)
    {
        TInstance.Draw(this, transform, graphics);
    }
}

public static class SpriteBatchExtensions
{
    extension(Graphics graphics)
    {
        public void DrawSpriteBatch<T>(SpriteBatch<T> spriteBatch)
            where T : unmanaged, ISpriteInstance<T>
        {
            spriteBatch.Draw(new Transform(), graphics);
        }

        public void DrawSpriteBatch<T>(in Transform transform, SpriteBatch<T> spriteBatch)
            where T : unmanaged, ISpriteInstance<T>
        {
            spriteBatch.Draw(transform, graphics);
        }
    }
}

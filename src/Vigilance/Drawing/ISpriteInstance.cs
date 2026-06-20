using Vigilance.Math;

namespace Vigilance.Drawing;

public interface ISpriteInstance<TSelf>
    where TSelf : unmanaged, ISpriteInstance<TSelf>
{
    static abstract Shader Shader { get; }
    static abstract void Draw(SpriteBatch<TSelf> batch, Transform transform, Graphics graphics);
}

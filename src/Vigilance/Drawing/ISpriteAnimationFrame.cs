namespace Vigilance.Drawing;

public interface ISpriteAnimationFrame
{
    TimeSpan Delay => TimeSpan.Zero;

    void UpdateSprite(Sprite sprite);
}

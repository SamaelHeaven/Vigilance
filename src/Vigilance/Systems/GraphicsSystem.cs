using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Math;

namespace Vigilance.Systems;

public sealed class GraphicsSystem(Graphics? graphics = null) : GameSystem
{
    public Graphics Graphics { get; set; } = graphics ?? Renderer.Graphics;

    public override void Configure()
    {
        Scene.OnAddOrSet<Sprite>(OnSetSprite);
    }

    public override void Render(Entity entity)
    {
        Graphics.DrawEntity(entity);
    }

    private static void OnSetSprite(Entity entity, Sprite sprite)
    {
        if (entity.Scale == Vector2.One)
            entity.Scale = sprite.Texture.Size;
    }
}

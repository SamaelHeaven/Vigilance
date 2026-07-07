using Vigilance.Core;
using Vigilance.Drawing;
using Vigilance.Physics;

namespace Vigilance.Systems;

public sealed class PhysicsSystem() : GameSystem(queryWithDisabled: true)
{
    public Graphics DebugDrawGraphics { get; set; } = Renderer.Graphics;
    public bool IsDebugDrawEnabled { get; set; } = false;
    public DebugDrawFlags DebugDrawFlags { get; set; } = DebugDrawFlags.Default;

    public override void Configure()
    {
        Scene.OnAddOrSet<Body>(SetBody);
        Scene.OnRemove<Body>(RemoveBody);
    }

    public override void FixedUpdate()
    {
        Scene.World.Update();
        foreach (var (entity, body) in Entries<Body>())
        {
            if (entity != body.Entity)
                continue;
            var transform = body.Transform;
            entity.Position = transform.Position;
            entity.Rotation = transform.Rotation;
        }
    }

    public override void PostRender()
    {
        if (IsDebugDrawEnabled)
            Scene.World.DebugDraw(DebugDrawGraphics, DebugDrawFlags, Scene.Camera);
    }

    private static void SetBody(Entity entity, Body body)
    {
        body.Entity = entity;
        var transform = body.Transform;
        entity.Position = transform.Position;
        entity.Rotation = transform.Rotation;
    }

    private static void RemoveBody(Entity entity, Body body)
    {
        if (entity != body.Entity)
            return;
        body.Entity = Entity.Null;
        body.Destroy();
    }
}

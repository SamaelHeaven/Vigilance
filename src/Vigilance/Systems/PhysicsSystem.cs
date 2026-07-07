using Vigilance.Core;

namespace Vigilance.Physics;

public sealed class PhysicsSystem() : GameSystem(queryWithDisabled: true)
{
    public override void Configure()
    {
        Scene.OnAddOrSet<Body>(SetBody);
        Scene.OnRemove<Body>(RemoveBody);
    }

    public override void FixedUpdate()
    {
        Scene.World.Update();
        foreach (var body in Components<Body>())
        {
            var entity = body.Entity;
            entity.AssertValid();
            var transform = body.Transform;
            entity.Position = transform.Position;
            entity.Rotation = transform.Rotation;
        }
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
        body.Entity = Entity.Null;
        body.Destroy();
    }
}

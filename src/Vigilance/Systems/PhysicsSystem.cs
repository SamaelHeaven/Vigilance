namespace Vigilance.Systems;

public sealed class PhysicsSystem : GameSystem
{
    private Table<Body> _bodies = null!;

    public PhysicsSystem()
        : base(queryWithDisabled: true)
    {
        Order = 1;
    }

    public Graphics DebugDrawGraphics { get; set; } = Renderer.Graphics;
    public bool IsDebugDrawEnabled { get; set; } = false;
    public DebugDrawFlags DebugDrawFlags { get; set; } = DebugDrawFlags.Default;

    public override void Configure()
    {
        _bodies = Scene.Table<Body>();
        Scene.OnAddOrSet<Body>(SetBody);
        Scene.OnRemove<Body>(RemoveBody);
        Scene.OnAdd<Disabled>(Disable);
        Scene.OnRemove<Disabled>(Enable);
        Scene.OnSet<Position>(SetPosition);
        Scene.OnSet<Rotation>(SetRotation);
    }

    public override void FixedUpdate()
    {
        Scene.World.Update();
        foreach (var (entity, bodyRef) in RefEntries<Body>())
        {
            var body = bodyRef.Read;
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
        body.Transform = (entity.Position, entity.Rotation);
        body.IsEnabled = !entity.IsDisabled;
    }

    private static void RemoveBody(Entity entity, Body body)
    {
        if (entity != body.Entity)
            return;
        body.Entity = Entity.Null;
        body.Destroy();
    }

    private void SetPosition(Entity entity, Position position)
    {
        if (_bodies.TryGet(entity, out var body) && body.Entity == entity)
            body.Position = position;
    }

    private void SetRotation(Entity entity, Rotation rotation)
    {
        if (_bodies.TryGet(entity, out var body) && body.Entity == entity)
            body.Rotation = rotation;
    }

    private void Disable(Entity entity, Disabled disabled)
    {
        if (_bodies.TryGet(entity, out var body) && body.Entity == entity)
            body.IsEnabled = false;
    }

    private void Enable(Entity entity, Disabled disabled)
    {
        if (_bodies.TryGet(entity, out var body) && body.Entity == entity)
            body.IsEnabled = true;
    }
}

using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Physics;

public record struct WorldDef
{
    public WorldDef()
    {
        Gravity = World.DefaultGravity;
        Multithreaded = World.DefaultMultithreaded;
    }

    public Scene? Scene { get; set; }
    public Vector2 Gravity { get; set; }
    public bool Multithreaded { get; set; }
    internal bool Internal { get; set; }
}

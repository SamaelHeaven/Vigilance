using Vigilance.Math;

namespace Vigilance.Core;

public struct Camera
{
    internal static GameSystem System =>
        scene =>
        {
            scene.OnInitialize(() =>
            {
                scene.Set(new Camera());
            });

            scene.OnRemove<Camera>(
                (entity, _) =>
                {
                    if (entity.IsSingleton)
                        throw new InvalidOperationException("Cannot remove camera from scene.");
                }
            );
        };

    public Vector2 Target = Vector2.Zero;
    public Vector2 Offset = Vector2.Zero;
    public float Rotation = 0;
    public float Zoom = 1;

    public Camera() { }
}

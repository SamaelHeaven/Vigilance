using Vigilance.Core;

namespace Vigilance.Systems;

public struct CameraSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnInitialize(() =>
        {
            scene.Set(new Camera());
        });

        scene.OnRemove<Camera>(static entity =>
        {
            if (entity.IsSingleton)
                throw new InvalidOperationException("Cannot remove camera from scene.");
        });
    }
}

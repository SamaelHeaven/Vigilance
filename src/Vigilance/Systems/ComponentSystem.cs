using Vigilance.Core;

namespace Vigilance.Systems;

public sealed class ComponentSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnUpdate(() =>
        {
            scene.Each(
                (Entity entity, Components components) =>
                {
                    foreach (var component in components.OfType<IComponent>())
                        component.Update(entity);
                }
            );
        });

        scene.OnFixedUpdate(() =>
        {
            scene.Each(
                (Entity entity, Components components) =>
                {
                    foreach (var component in components.OfType<IComponent>())
                        component.FixedUpdate(entity);
                }
            );
        });

        scene.OnRender(entity =>
        {
            foreach (var component in entity.Components.OfType<IComponent>())
                component.Render(entity);
        });
    }
}

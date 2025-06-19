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
                    foreach (var component in components.OfType<Component>())
                        component.Update(entity);
                }
            );
        });

        scene.OnFixedUpdate(() =>
        {
            scene.Each(
                (Entity entity, Components components) =>
                {
                    foreach (var component in components.OfType<Component>())
                        component.FixedUpdate(entity);
                }
            );
        });

        scene.OnRenderBegin(() =>
        {
            scene.Each(
                (Entity entity, Components components) =>
                {
                    foreach (var component in components.OfType<Component>())
                        component.RenderBegin(entity);
                }
            );
        });

        scene.OnRender(entity =>
        {
            foreach (var component in entity.Components.OfType<Component>())
                component.Render(entity);
        });

        scene.OnRenderEnd(() =>
        {
            scene.Each(
                (Entity entity, Components components) =>
                {
                    foreach (var component in components.OfType<Component>())
                        component.RenderEnd(entity);
                }
            );
        });
    }
}

using Vigilance.Core;

namespace Vigilance.Systems;

public struct YSortSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnRenderStart(() =>
        {
            scene.Each(
                static (Entity entity, ref YSort ySort) =>
                {
                    entity.ZIndex = (int)MathF.Round(entity.WorldPosition.Y + ySort.Offset);
                }
            );
        });
    }
}

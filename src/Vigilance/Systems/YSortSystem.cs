using Vigilance.Core;

namespace Vigilance.Systems;

public sealed class YSortSystem(float offset = 0) : ISystem
{
    public float Offset { get; set; } = offset;

    public void Configure(Scene scene)
    {
        scene.OnRenderBegin(() =>
        {
            scene.Each(
                (Entity entity, YSort ySort) =>
                {
                    entity.ZIndex = (int)MathF.Round(entity.WorldPosition.Y + Offset + ySort.Offset);
                }
            );
        });
    }
}

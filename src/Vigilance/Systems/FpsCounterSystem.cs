using Vigilance.Core;
using Vigilance.Drawing;

namespace Vigilance.Systems;

public struct FpsCounter
{
    public TimeSpan ElapsedTime = TimeSpan.Zero;
    public TimeSpan RefreshRate = TimeSpan.Zero;
    public float FpsCount = 0;
    public int FrameCount = 0;

    public FpsCounter() { }
}

public struct FpsCounterPrefab : IPrefab
{
    public TimeSpan RefreshRate = TimeSpan.FromSeconds(0.5);

    public Text Text = new()
    {
        Fill = Color.Green,
        Stroke = Color.DarkGreen,
        StrokeWidth = 2,
        Camera = null,
    };

    public FpsCounterPrefab() { }

    public void Build(Entity entity)
    {
        entity.Set(new FpsCounter { RefreshRate = RefreshRate }).Set(Text);
    }
}

public struct FpsCounterSystem : ISystem
{
    public void Configure(Scene scene)
    {
        scene.OnUpdate(() =>
        {
            switch (Game.Debug)
            {
                case false:
                    scene.Remove<FpsCounter>();
                    return;
                case true when !scene.Has<FpsCounter>():
                    scene.Set(new FpsCounter());
                    new FpsCounterPrefab().Build(scene.Singleton<FpsCounter>());
                    break;
            }

            scene.Each(
                (Entity entity, ref FpsCounter fpsCounter, ref Text text) =>
                {
                    fpsCounter.FrameCount++;
                    fpsCounter.FpsCount += Time.CurrentFps;
                    fpsCounter.ElapsedTime = fpsCounter.ElapsedTime.Add(TimeSpan.FromSeconds(Time.Delta));
                    if (text.Value != "" && fpsCounter.ElapsedTime <= fpsCounter.RefreshRate)
                        return;
                    text.Value = $"FPS: {(int)MathF.Round(fpsCounter.FpsCount / fpsCounter.FrameCount)}";
                    entity.Position = text.Size / 2 + 4;
                    fpsCounter.ElapsedTime = TimeSpan.Zero;
                    fpsCounter.FpsCount = 0f;
                    fpsCounter.FrameCount = 0;
                }
            );
        });
    }
}

namespace Vigilance.Drawing;

public interface IAnimation
{
    bool IsPaused { get; set; }

    void Update(TimeSpan? step = null);

    void Apply(Entity entity);

    void Reset();
}

namespace Vigilance.Core;

public interface IComponent
{
    void Update(Entity entity) { }

    void FixedUpdate(Entity entity) { }

    void Render(Entity entity) { }
}

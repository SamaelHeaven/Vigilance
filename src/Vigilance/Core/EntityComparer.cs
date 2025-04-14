namespace Vigilance.Core;

public struct EntityComparer(Comparison<Entity> comparison) : IComparer<Entity>
{
    public Comparison<Entity> Comparison = comparison;

    public int Compare(Entity x, Entity y)
    {
        return Comparison.Invoke(x, y);
    }
}

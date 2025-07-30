using System.Text;

namespace Vigilance.Core;

public readonly record struct Component(Type Type, object? Data = null)
{
    public bool Equals(Component other)
    {
        return Type == other.Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type);
    }

    private bool PrintMembers(StringBuilder sb)
    {
        var type = Data?.GetType();
        if (Type != type)
        {
            sb.Append("Type = ");
            sb.Append(Type.Name);
            sb.Append(", Data = ");
        }

        sb.Append(Data);
        return true;
    }
}

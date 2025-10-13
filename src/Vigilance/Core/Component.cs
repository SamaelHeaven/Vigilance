using System.Runtime.CompilerServices;
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
        return Type.GetHashCode();
    }

    public bool TryGet<T>(out T t)
    {
        if (Type != typeof(T))
        {
            Unsafe.SkipInit(out t);
            return false;
        }

        t = (T)Data!;
        return true;
    }

    public bool TryCast<T>(out T t)
    {
        if (Data is not T value)
        {
            Unsafe.SkipInit(out t);
            return false;
        }

        t = value;
        return true;
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

namespace Vigilance.Core;

public readonly record struct Name(string Value) : IImmutableComponent, ISkipAddEventComponent, ISkipSetEventComponent
{
    public static implicit operator string(Name name)
    {
        return name.Value;
    }

    public static implicit operator Name(string name)
    {
        return new Name(name);
    }
}

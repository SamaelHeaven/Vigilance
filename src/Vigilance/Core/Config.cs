using ZLinq;

namespace Vigilance.Core;

public sealed class Config
{
    private readonly Dictionary<Type, Entry> _configs;

    internal Config(Dictionary<Type, Entry> configs)
    {
        _configs = configs.AsValueEnumerable().ToDictionary();
    }

    public static Config Empty { get; } = new(new Dictionary<Type, Entry>());

    public static ConfigBuilder Builder()
    {
        return new ConfigBuilder();
    }

    public T? Take<T>()
    {
        var type = typeof(T);
        if (!_configs.Remove(type, out var config))
            return default;
        var obj = (T)config.Object;
        var action = (Action<T>)config.Action;
        action.Invoke(obj);
        return obj;
    }

    public bool TryTake<T>(out T config)
    {
        config = Take<T>()!;
        return (T?)config is not null;
    }

    public bool Has<T>()
    {
        var type = typeof(T);
        return _configs.ContainsKey(type);
    }

    internal readonly record struct Entry(object Object, Delegate Action);
}

public sealed class ConfigBuilder
{
    private readonly Dictionary<Type, Config.Entry> _configs = new();

    internal ConfigBuilder() { }

    public ConfigBuilder Add<T>(Action<T> config)
        where T : new()
    {
        _configs.Add(typeof(T), new Config.Entry(new T(), config));
        return this;
    }

    public Config Build()
    {
        return new Config(_configs);
    }
}

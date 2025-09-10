namespace Vigilance.Core;

public sealed class Configs(IEnumerable<KeyValuePair<Type, object>> configs)
{
    private readonly Dictionary<Type, object> _configs = configs
        .Select(config => (config.Key, Cloner.MemberwiseClone(config.Value)))
        .ToDictionary();

    public static Configs Empty { get; } = new(Enumerable.Empty<KeyValuePair<Type, object>>());

    public static ConfigsBuilder Builder()
    {
        return new ConfigsBuilder();
    }

    public T? Take<T>()
    {
        var type = typeof(T);
        if (_configs.Remove(type, out var config))
            return (T)config;
        return default;
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
}

public sealed class ConfigsBuilder
{
    private readonly Dictionary<Type, object> _configs = new();

    internal ConfigsBuilder() { }

    public ConfigsBuilder AddConfig(object config)
    {
        var type = config.GetType();
        _configs[type] = config;
        return this;
    }

    public Configs Build()
    {
        return new Configs(_configs);
    }
}

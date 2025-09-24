using ZLinq;

namespace Vigilance.Core;

public sealed class Config
{
    private readonly Dictionary<Type, object> _configs;

    internal Config(IEnumerable<KeyValuePair<Type, object>> configs)
    {
        _configs = configs
            .AsValueEnumerable()
            .Select(config => (config.Key, Cloner.MemberwiseClone(config.Value)))
            .ToDictionary();
    }

    public static Config Empty { get; } = new(Enumerable.Empty<KeyValuePair<Type, object>>());

    public static ConfigBuilder Builder()
    {
        return new ConfigBuilder();
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

public sealed class ConfigBuilder
{
    private readonly Dictionary<Type, object> _configs = new();

    internal ConfigBuilder() { }

    public ConfigBuilder Add(object config)
    {
        var type = config.GetType();
        _configs[type] = config;
        return this;
    }

    public Config Build()
    {
        return new Config(_configs);
    }
}

using Vigilance.Collections;

namespace Vigilance.Core;

public sealed class Config
{
    private ValueDictionary<Type, Entry> _configs;

    internal Config(in ValueDictionary<Type, Entry> configs)
    {
        _configs = configs.AsValueEnumerable().ToValueDictionary();
    }

    public static Config Empty { get; } = new([]);

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
        action.SafeInvoke(obj);
        return obj;
    }

    internal record struct Entry(object Object, Delegate Action);
}

public sealed class ConfigBuilder
{
    private ValueDictionary<Type, Config.Entry> _configs = [];

    internal ConfigBuilder() { }

    public ConfigBuilder Add<T>(Action<T> config)
        where T : new()
    {
        ref var entry = ref _configs.GetValueRefOrAddDefault(typeof(T), out var exists);
        if (!exists)
        {
            entry = new Config.Entry(new T(), config);
            return this;
        }

        entry.Action = Delegate.Combine(entry.Action, config);
        return this;
    }

    public Config Create()
    {
        return new Config(_configs);
    }
}

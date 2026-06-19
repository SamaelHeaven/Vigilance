using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Vigilance.Core;

public static class Asset
{
    private static AssetConfig _config = new();

    public static CacheType DefaultCacheType
    {
        get => _config.DefaultCacheType;
        set => _config.DefaultCacheType = value;
    }

    internal static void Initialize()
    {
        _config = Game.Config.Take<AssetConfig>() ?? _config;
    }

    public sealed class Container<TKey, TValue>
        where TKey : notnull
        where TValue : class
    {
        private readonly Dictionary<TKey, TValue> _strongFiles = new();
        private readonly Dictionary<TKey, TValue> _strongResources = new();
        private readonly Dictionary<TKey, TValue> _strongValues = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakFiles = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakResources = new();
        private readonly Dictionary<TKey, WeakReference<TValue>> _weakValues = new();

        public bool File(
            ref string path,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            var filePath = FileSystem.FormatPath(path);
            var normalizedPath = FileSystem.NormalizePath(path);
            path = normalizedPath;
            return File(
                keyFunc,
                () => FileSystem.TryReadBytes(filePath, out var bytes) ? valueFunc.Invoke(bytes) : null,
                cacheType,
                out value
            );
        }

        public bool File(
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            return Get(_weakFiles, _strongFiles, keyFunc, valueFunc, cacheType, out value);
        }

        public bool Resource(
            ref string resource,
            string? @namespace,
            Assembly? assembly,
            Func<TKey> keyFunc,
            Func<byte[], TValue> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            resource = Core.Resource.Format(resource, @namespace);
            var resourceValue = resource;
            resource = Core.Resource.Format(resource, assembly?.FullName ?? "");
            return Resource(
                keyFunc,
                () =>
                    Core.Resource.TryReadBytes(resourceValue, out var bytes, "", assembly)
                        ? valueFunc.Invoke(bytes)
                        : null,
                cacheType,
                out value
            );
        }

        public bool Resource(
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            return Get(_weakResources, _strongResources, keyFunc, valueFunc, cacheType, out value);
        }

        public bool Raw(TKey key, Func<TValue> valueFunc, CacheType? cacheType, [MaybeNullWhen(false)] out TValue value)
        {
            return Get(_weakValues, _strongValues, () => key, valueFunc, cacheType, out value);
        }

        private static bool Get(
            Dictionary<TKey, WeakReference<TValue>> weakValues,
            Dictionary<TKey, TValue> strongValues,
            Func<TKey> keyFunc,
            Func<TValue?> valueFunc,
            CacheType? cacheType,
            [MaybeNullWhen(false)] out TValue value
        )
        {
            try
            {
                var key = keyFunc.Invoke();
                var cacheTypeValue = cacheType ?? DefaultCacheType;
                var weak = cacheTypeValue == CacheType.Weak;
                var strong = cacheTypeValue == CacheType.Strong;
                if (weak || strong)
                {
                    if (weakValues.TryGetValue(key, out var reference))
                        if (reference.TryGetTarget(out value!))
                            return true;
                    if (strongValues.TryGetValue(key, out value!))
                        return true;
                }

                value = valueFunc.Invoke();
                if (value is null)
                    return false;
                if (weak)
                    weakValues[key] = new WeakReference<TValue>(value);
                if (strong)
                    strongValues[key] = value;
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}

public sealed class AssetConfig
{
    public CacheType DefaultCacheType { get; set; } = CacheType.Weak;
}

public static class AssetConfigExtensions
{
    public static ConfigBuilder Asset(this ConfigBuilder builder, Action<AssetConfig> config)
    {
        return builder.Add(config);
    }
}

public class AssetException : Exception
{
    public AssetException(string message)
        : base(message) { }
}

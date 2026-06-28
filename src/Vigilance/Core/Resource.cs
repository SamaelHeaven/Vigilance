using System.Reflection;
using System.Text;
using Vigilance.Collections;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Core;

public static class Resource
{
    private static ValueDictionary<Assembly, HashSet<string>> _resourceNames = [];

    static Resource()
    {
        WorkingNamespace = new ResourceConfig().WorkingNamespace;
    }

    public static string WorkingNamespace { get; set; }

    internal static void Initialize()
    {
        var config = Game.Config.Take<ResourceConfig>() ?? new ResourceConfig();
        WorkingNamespace = config.WorkingNamespace;
    }

    public static string Format(string resource, string? @namespace = null)
    {
        @namespace ??= WorkingNamespace;
        return @namespace.IsEmpty ? resource : $"{@namespace}.{resource}";
    }

    public static string Format(string resource, string? @namespace, Assembly? assembly)
    {
        @namespace ??= WorkingNamespace;
        assembly ??= Assemblies.Game;
        return $"{assembly.GetName().Name}.{@namespace}{(@namespace.IsEmpty ? "" : ".")}{resource}";
    }

    public static bool Exists(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        assembly ??= Assemblies.Game;
        resource = Format(resource, @namespace, assembly);
        ref var names = ref _resourceNames.GetValueRefOrAddDefault(assembly, out var exists)!;
        if (exists)
            return names.Contains(resource);
        names = assembly.GetManifestResourceNames().AsValueEnumerable().ToHashSet();
        return names.Contains(resource);
    }

    public static bool TryReadText(
        string resource,
        out string text,
        string? @namespace = null,
        Assembly? assembly = null
    )
    {
        var result = TryReadBytes(resource, out var bytes, @namespace, assembly);
        text = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public static string ReadText(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        if (!TryReadText(resource, out var text, @namespace, assembly))
            Log.Warning($"FILEIO: [{Format(resource, @namespace)}] Failed to read resource text");
        return text;
    }

    public static bool TryReadBytes(
        string resource,
        out byte[] bytes,
        string? @namespace = null,
        Assembly? assembly = null
    )
    {
        assembly ??= Assemblies.Game;
        resource = Format(resource, @namespace, assembly);
        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
        {
            bytes = [];
            return false;
        }

        if (stream.CanSeek)
        {
            var length = (int)stream.Length;
            bytes = new byte[length];
            stream.ReadExactly(bytes);
            return true;
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        bytes = ms.ToArray();
        return true;
    }

    public static byte[] ReadBytes(string resource, string? @namespace = null, Assembly? assembly = null)
    {
        if (!TryReadBytes(resource, out var bytes, @namespace, assembly))
            Log.Warning($"FILEIO: [{Format(resource, @namespace)}] Failed to read resource");
        return bytes;
    }
}

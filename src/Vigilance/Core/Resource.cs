using System.Reflection;
using System.Text;
using Vigilance.Collections;
using Vigilance.Logging;
using ZLinq;

namespace Vigilance.Core;

public readonly record struct Resource(string Name, Assembly Assembly)
{
    private static ValueDictionary<Assembly, ValueHashSet<string>> _resourceNames = [];

    public static implicit operator Resource(string name)
    {
        return new Resource(name, Assemblies.Game);
    }

    public static implicit operator Resource(in (string Name, Assembly Assembly) resource)
    {
        return new Resource(resource.Name, resource.Assembly);
    }

    public static bool Exists(in Resource resource)
    {
        if (resource.Name.IsEmpty)
            return false;
        ref var names = ref _resourceNames.GetValueRefOrAddDefault(resource.Assembly, out var exists)!;
        if (!exists)
            names = resource.Assembly.GetManifestResourceNames().AsValueEnumerable().ToValueHashSet();
        return names.Contains(resource.Name);
    }

    public static bool TryReadText(in Resource resource, out string text)
    {
        var result = TryReadBytes(resource, out var bytes);
        text = Encoding.UTF8.GetString(bytes);
        return result;
    }

    public static string ReadText(in Resource resource)
    {
        if (!TryReadText(resource, out var text))
            Log.Warning($"FILEIO: [{resource}] Failed to read resource text");
        return text;
    }

    public static bool TryReadBytes(in Resource resource, out byte[] bytes)
    {
        if (!Exists(resource))
        {
            bytes = [];
            return false;
        }

        using var stream = resource.Assembly.GetManifestResourceStream(resource.Name);
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

    public static byte[] ReadBytes(in Resource resource)
    {
        if (!TryReadBytes(resource, out var bytes))
            Log.Warning($"FILEIO: [{resource}] Failed to read resource");
        return bytes;
    }
}

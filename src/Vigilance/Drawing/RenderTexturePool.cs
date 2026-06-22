using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;

namespace Vigilance.Drawing;

internal static class RenderTexturePool
{
    private static readonly TimeSpan _lifetime = TimeSpan.FromSeconds(6);
    private static ValueList<(RenderTexture Texture, TimeSpan Time)> _entries = [];

    public static bool TryRent(
        int width,
        int height,
        out RenderTexture2D renderTexture2D,
        out int physicalWidth,
        out int physicalHeight
    )
    {
        RenderTexture? best = null;
        var bestIndex = -1;
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            var candidate = entry.Texture;
            if (candidate.PhysicalWidth < width || candidate.PhysicalHeight < height)
                continue;
            if (
                best is not null
                && (long)candidate.PhysicalWidth * candidate.PhysicalHeight
                    >= (long)best.PhysicalWidth * best.PhysicalHeight
            )
                continue;
            best = candidate;
            bestIndex = i;
        }

        if (best is null)
        {
            renderTexture2D = default;
            physicalWidth = 0;
            physicalHeight = 0;
            return false;
        }

        _entries[bestIndex] = _entries[^1];
        _entries.RemoveAt(_entries.Count - 1);
        best.DetachForReuse(out renderTexture2D, out physicalWidth, out physicalHeight);
        return true;
    }

    public static void Return(RenderTexture renderTexture)
    {
        _entries.Add((renderTexture, Time.Elapsed));
    }

    internal static void Update()
    {
        var now = Time.Elapsed;
        _entries.RemoveAll(entry =>
        {
            if (now - entry.Time <= _lifetime)
                return false;
            entry.Texture.DetachForReuse(out var renderTexture2D, out _, out _);
            Raylib.UnloadRenderTexture(renderTexture2D);
            return true;
        });
    }
}

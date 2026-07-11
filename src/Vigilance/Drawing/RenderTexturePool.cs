using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;

namespace Vigilance.Drawing;

internal static class RenderTexturePool
{
    private static ValueList<(
        RenderTexture2D Texture,
        int PhysicalWidth,
        int PhysicalHeight,
        TimeSpan Time
    )> _entries = [];

    public static bool TryRent(
        int width,
        int height,
        out RenderTexture2D renderTexture2D,
        out int physicalWidth,
        out int physicalHeight
    )
    {
        var bestIndex = -1;
        for (var i = 0; i < _entries.Count; i++)
        {
            var candidate = _entries[i];
            if (candidate.PhysicalWidth < width || candidate.PhysicalHeight < height)
                continue;
            if (
                bestIndex >= 0
                && (long)candidate.PhysicalWidth * candidate.PhysicalHeight
                    >= (long)_entries[bestIndex].PhysicalWidth * _entries[bestIndex].PhysicalHeight
            )
                continue;
            bestIndex = i;
        }

        if (bestIndex < 0)
        {
            renderTexture2D = default;
            physicalWidth = 0;
            physicalHeight = 0;
            return false;
        }

        var best = _entries[bestIndex];
        _entries[bestIndex] = _entries[^1];
        _entries.RemoveAt(_entries.Count - 1);
        renderTexture2D = best.Texture;
        physicalWidth = best.PhysicalWidth;
        physicalHeight = best.PhysicalHeight;
        return true;
    }

    public static void Return(RenderTexture2D renderTexture2D, int physicalWidth, int physicalHeight)
    {
        if (renderTexture2D.Texture.Id == 0)
            return;
        _entries.Add((renderTexture2D, physicalWidth, physicalHeight, Time.Elapsed));
    }

    internal static void Update()
    {
        var now = Time.Elapsed;
        _entries.RemoveAll(entry =>
        {
            if (now - entry.Time <= Drawing.RenderTexturePoolLifetime)
                return false;
            Raylib.UnloadRenderTexture(entry.Texture);
            return true;
        });
    }
}

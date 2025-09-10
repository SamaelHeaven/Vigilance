using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Drawing;

public sealed class FontConfig
{
    public int DefaultQuality { get; set; } = 128;
    public int DefaultSize { get; set; } = 16;
    public TextHeightMode DefaultTextHeightMode { get; set; } = TextHeightMode.Character;
    public Vector2 DefaultTextSpacing { get; set; } = new(0, 4);

    public string DefaultCharset { get; set; } =
        "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    public Func<Font> Default { get; set; } =
        () =>
        {
            var assembly = Assemblies.Engine;
            return Asset.FontResource(
                "Font.Default.ttf",
                @namespace: $"{assembly.GetName().Name}.Resources",
                assembly: assembly
            );
        };
}

public static class FontConfigExtensions
{
    public static ConfigsBuilder Font(this ConfigsBuilder builder, FontConfig config)
    {
        return builder.AddConfig(config);
    }
}

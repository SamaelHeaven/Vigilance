using Raylib_cs;

namespace Vigilance.Audio;

public static class Audio
{
    private static AudioConfig _config = new();

    public static float MasterVolume
    {
        get;
        set
        {
            value = value.Clamp(0, 1);
            if (!Game.Running || Precision.AreEqual(value, field))
                return;
            field = value;
            Raylib.SetMasterVolume(value);
        }
    } = _config.MasterVolume;

    public static int DefaultSoundMaxAliases
    {
        get;
        set => field = value.Max(1);
    } = _config.DefaultSoundMaxAliases;

    internal static void Initialize()
    {
        if (OperatingSystem.IsWindows())
        {
            var thread = new Thread(Raylib.InitAudioDevice);
            thread.Start();
            thread.Join();
        }
        else
        {
            Raylib.InitAudioDevice();
        }

        _config = Game.Config.Take<AudioConfig>() ?? _config;
        MasterVolume = _config.MasterVolume;
        DefaultSoundMaxAliases = _config.DefaultSoundMaxAliases;
    }

    internal static void Dispose()
    {
        Raylib.CloseAudioDevice();
    }
}

public sealed class AudioConfig
{
    public float MasterVolume { get; set; } = 1f;

    public int DefaultSoundMaxAliases { get; set; } = 16;
}

public static class AudioConfigExtensions
{
    public static ConfigBuilder Audio(this ConfigBuilder builder, Action<AudioConfig> config)
    {
        return builder.Add(config);
    }
}

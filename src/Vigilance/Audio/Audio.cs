using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Audio;

public static class Audio
{
    private static AudioConfig _config = new();

    public static float MasterVolume
    {
        get => _config.MasterVolume;
        set
        {
            value = value.Clamp(0, 1);
            _config.MasterVolume = value;
            if (Game.Running)
                Raylib.SetMasterVolume(value);
        }
    }

    public static int DefaultSoundMaxAliases
    {
        get => _config.DefaultSoundMaxAliases;
        set => _config.DefaultSoundMaxAliases = int.Max(value, 1);
    }

    internal static void Initialize()
    {
        Raylib.SetAudioStreamBufferSizeDefault(8192);
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

        if (Game.Config.TryTake(out AudioConfig config))
            _config = config;
        MasterVolume = _config.MasterVolume;
        DefaultSoundMaxAliases = _config.DefaultSoundMaxAliases;
    }
}

public sealed class AudioConfig
{
    public float MasterVolume { get; set; } = 1f;

    public int DefaultSoundMaxAliases { get; set; } = 16;
}

public static class AudioConfigExtensions
{
    public static ConfigBuilder Audio(this ConfigBuilder builder, AudioConfig config)
    {
        return builder.Add(config);
    }
}

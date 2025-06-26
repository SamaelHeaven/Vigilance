using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Audio;

public sealed class Sound
{
    private static readonly List<Sound> Sounds = [];
    private readonly List<(Raylib_cs.BleedingEdge.Sound Sound, double LastUsed)> _aliases = [];
    private readonly Raylib_cs.BleedingEdge.Sound _sound;
    private float _pan = 0.5f;
    private float _pitch = 1;
    private float _volume = 1;

    public unsafe Sound(string fileType, IEnumerable<byte> bytes, int? maxAliases = null)
    {
        Game.EnsureRunning();
        MaxAliases = System.Math.Max(maxAliases ?? Game.DefaultSoundMaxAliases, 1);
        using var fileTypeBuffer = fileType.ToUtf8Buffer();
        var span = bytes.AsSpan();
        fixed (byte* bytesBuffer = span)
        {
            var wave = Raylib.LoadWaveFromMemory(fileTypeBuffer.AsPointer(), bytesBuffer, span.Length);
            _sound = Raylib.LoadSoundFromWave(wave);
            Raylib.UnloadWave(wave);
        }
    }

    public int MaxAliases { get; }

    public float Volume
    {
        get => _volume;
        set
        {
            value = System.Math.Clamp(value, 0, 1);
            if (Precision.AreEqual(value, Volume))
                return;
            _volume = value;
            Raylib.SetSoundVolume(_sound, value);
            foreach (var (sound, _) in _aliases)
                Raylib.SetSoundVolume(sound, value);
        }
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            if (Precision.AreEqual(value, Pitch))
                return;
            _pitch = value;
            Raylib.SetSoundPitch(_sound, value);
            foreach (var (sound, _) in _aliases)
                Raylib.SetSoundPitch(sound, value);
        }
    }

    public float Pan
    {
        get => _pan;
        set
        {
            value = System.Math.Clamp(value, 0, 1);
            if (Precision.AreEqual(value, Pan))
                return;
            _pan = value;
            Raylib.SetSoundPan(_sound, value);
            foreach (var (sound, _) in _aliases)
                Raylib.SetSoundPan(sound, value);
        }
    }

    public bool Playing => _aliases.Any(a => Raylib.IsSoundPlaying(a.Sound));

    public bool Stopped => !Playing;

    public Sound SetVolume(float volume)
    {
        Volume = volume;
        return this;
    }

    public Sound SetPitch(float pitch)
    {
        Pitch = pitch;
        return this;
    }

    public Sound SetPan(float pan)
    {
        Pan = pan;
        return this;
    }

    public Sound Play()
    {
        Raylib_cs.BleedingEdge.Sound alias;
        var now = Time.Elapsed.TotalSeconds;
        var index = _aliases.FindIndex(a => !Raylib.IsSoundPlaying(a.Sound));
        if (index != -1)
        {
            alias = _aliases[index].Sound;
            _aliases[index] = (alias, now);
        }
        else if (_aliases.Count < MaxAliases)
        {
            alias = Raylib.LoadSoundAlias(_sound);
            _aliases.Add((alias, now));
        }
        else
        {
            var oldestIndex = 0;
            var oldestTime = _aliases[0].LastUsed;
            for (var i = 1; i < _aliases.Count; i++)
                if (_aliases[i].LastUsed < oldestTime)
                {
                    oldestIndex = i;
                    oldestTime = _aliases[i].LastUsed;
                }

            alias = _aliases[oldestIndex].Sound;
            _aliases[oldestIndex] = (alias, now);
        }

        Raylib.PlaySound(alias);
        if (!Sounds.Contains(this))
            Sounds.Add(this);
        return this;
    }

    public Sound Stop()
    {
        foreach (var (sound, _) in _aliases)
            if (Raylib.IsSoundPlaying(sound))
                Raylib.StopSound(sound);
        return this;
    }

    internal static void UpdateAll()
    {
        for (var i = Sounds.Count - 1; i >= 0; i--)
        {
            var sound = Sounds[i];
            if (!sound.Playing)
                Sounds.RemoveAt(i);
        }
    }

    ~Sound()
    {
        Game.Defer(() =>
        {
            foreach (var (sound, _) in _aliases)
                Raylib.UnloadSoundAlias(sound);
            Raylib.UnloadSound(_sound);
        });
    }
}

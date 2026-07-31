using Raylib_cs;

namespace Vigilance.Audio;

public sealed class Sound : IDisposable
{
    private static ValueList<Sound> _sounds = [];
    private ValueList<(Raylib_cs.Sound Sound, double LastUsed)> _aliases = [];
    private float _pan = 0.5f;
    private float _pitch = 1;
    private Raylib_cs.Sound _sound;
    private float _volume = 1;

    public unsafe Sound(in ReadOnlySpan<char> fileType, in ReadOnlySpan<byte> bytes, int? maxAliases = null)
    {
        Game.ThrowIfNotRunning();
        MaxAliases = maxAliases ?? Audio.DefaultSoundMaxAliases;
        using var fileTypeBuffer = fileType.ToUtf8Ptr();
        fixed (byte* bytesBuffer = bytes)
        {
            var wave = Raylib.LoadWaveFromMemory(fileTypeBuffer, bytesBuffer, bytes.Length);
            _sound = Raylib.LoadSoundFromWave(wave);
            Raylib.UnloadWave(wave);
        }
    }

    public int MaxAliases { get; private set; }

    public float Volume
    {
        get => _volume;
        set
        {
            value = value.Clamp(0, 1);
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
            value = value.Clamp(0, 1);
            if (Precision.AreEqual(value, Pan))
                return;
            _pan = value;
            Raylib.SetSoundPan(_sound, value);
            foreach (var (sound, _) in _aliases)
                Raylib.SetSoundPan(sound, value);
        }
    }

    public bool IsPlaying => _aliases.AsValueEnumerable().Any(a => Raylib.IsSoundPlaying(a.Sound));

    public bool IsStopped => !IsPlaying;

    public bool IsValid => _sound.Stream.Buffer != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        _aliases.Clear();
        _sound = default;
        _pan = 0;
        _pitch = 0;
        _volume = 0;
        MaxAliases = 0;
    }

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
        Raylib_cs.Sound alias;
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

        Raylib.SetSoundVolume(alias, _volume);
        Raylib.SetSoundPitch(alias, _pitch);
        Raylib.SetSoundPan(alias, _pan);
        Raylib.PlaySound(alias);
        if (!_sounds.Contains(this))
            _sounds.Add(this);
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
        for (var i = _sounds.Count - 1; i >= 0; i--)
        {
            var sound = _sounds[i];
            if (!sound.IsPlaying)
                _sounds.RemoveAt(i);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        foreach (var (sound, _) in _aliases)
            Raylib.UnloadSoundAlias(sound);
        Raylib.UnloadSound(_sound);
    }

    ~Sound()
    {
        Game.RunLater(ReleaseUnmanagedResources);
    }
}

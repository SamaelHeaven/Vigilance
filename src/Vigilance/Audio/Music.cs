using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Audio;

public sealed class Music : IDisposable
{
    private static readonly List<Music> _musics = [];
    private nint _buffer;
    private Raylib_cs.BleedingEdge.Music _music;
    private float _pan = 0.5f;
    private float _pitch = 1;
    private float _volume = 1;

    public unsafe Music(string fileType, IEnumerable<byte> bytes)
    {
        Game.EnsureRunning();
        using var fileTypeBuffer = fileType.ToUtf8Buffer();
        var span = bytes.AsSpan();
        _buffer = Marshal.AllocHGlobal(span.Length);
        fixed (byte* bytesBuffer = span)
        {
            Buffer.MemoryCopy(bytesBuffer, (byte*)_buffer, span.Length, span.Length);
        }

        _music = Raylib.LoadMusicStreamFromMemory(fileTypeBuffer.AsPointer(), (byte*)_buffer, span.Length);
    }

    public float Volume
    {
        get => _volume;
        set
        {
            value = value.Clamp(0, 1);
            if (Precision.AreEqual(value, Volume))
                return;
            _volume = value;
            Raylib.SetMusicVolume(_music, value);
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
            Raylib.SetMusicPitch(_music, value);
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
            Raylib.SetMusicPan(_music, value);
        }
    }

    public bool IsLooping
    {
        get => _music.Looping;
        set => _music.Looping = value;
    }

    public bool IsPaused { get; private set; }

    public bool IsPlaying => Raylib.IsMusicStreamPlaying(_music);

    public bool IsStopped => !IsPaused && !Raylib.IsMusicStreamPlaying(_music);

    public TimeSpan TimeLength => TimeSpan.FromSeconds(Raylib.GetMusicTimeLength(_music));

    public TimeSpan TimePlayed => TimeSpan.FromSeconds(Raylib.GetMusicTimePlayed(_music));

    public bool IsValid => _buffer != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        _buffer = 0;
        _music = default;
        _pan = 0;
        _pitch = 0;
        _volume = 0;
        IsPaused = false;
    }

    public Music SetVolume(float volume)
    {
        Volume = volume;
        return this;
    }

    public Music SetPitch(float pitch)
    {
        Pitch = pitch;
        return this;
    }

    public Music SetPan(float pan)
    {
        Pan = pan;
        return this;
    }

    public Music SetLooping(bool looping)
    {
        IsLooping = looping;
        return this;
    }

    public Music Play()
    {
        if (IsPaused)
        {
            Raylib.PlayMusicStream(_music);
            IsPaused = false;
            Stop();
        }
        else if (Raylib.IsMusicStreamPlaying(_music))
        {
            Stop();
        }

        Raylib.PlayMusicStream(_music);
        if (!_musics.Contains(this))
            _musics.Add(this);
        return this;
    }

    public Music Stop()
    {
        IsPaused = false;
        Raylib.StopMusicStream(_music);
        return this;
    }

    public Music Pause()
    {
        if (!Raylib.IsMusicStreamPlaying(_music))
            return this;
        IsPaused = true;
        Raylib.PauseMusicStream(_music);
        return this;
    }

    public Music Resume()
    {
        if (!IsPaused)
            return this;
        IsPaused = false;
        Raylib.ResumeMusicStream(_music);
        if (!_musics.Contains(this))
            _musics.Add(this);
        return this;
    }

    public Music Seek(TimeSpan time)
    {
        Raylib.SeekMusicStream(_music, (float)time.TotalSeconds);
        return this;
    }

    internal static void UpdateAll()
    {
        for (var i = _musics.Count - 1; i >= 0; i--)
        {
            var music = _musics[i];
            Raylib.UpdateMusicStream(music._music);
            if (!Raylib.IsMusicStreamPlaying(music._music))
                _musics.RemoveAt(i);
        }
    }

    private void ReleaseUnmanagedResources()
    {
        Raylib.UnloadMusicStream(_music);
        Marshal.FreeHGlobal(_buffer);
    }

    ~Music()
    {
        Game.Defer(ReleaseUnmanagedResources);
    }
}

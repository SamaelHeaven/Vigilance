using System.Runtime.InteropServices;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vigilance.Math;

namespace Vigilance.Media;

public sealed class Music
{
    private static readonly List<Music> Musics = [];
    private readonly nint _buffer;
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

    public bool Looping
    {
        get => _music.Looping;
        set => _music.Looping = value;
    }

    public bool Paused { get; private set; }

    public bool Playing => Raylib.IsMusicStreamPlaying(_music);

    public bool Stopped => !Paused && !Raylib.IsMusicStreamPlaying(_music);

    public TimeSpan TimeLength => TimeSpan.FromSeconds(Raylib.GetMusicTimeLength(_music));

    public TimeSpan TimePlayed => TimeSpan.FromSeconds(Raylib.GetMusicTimePlayed(_music));

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
        Looping = looping;
        return this;
    }

    public Music Play()
    {
        if (Paused)
        {
            Raylib.PlayMusicStream(_music);
            Paused = false;
            Stop();
        }
        else if (Raylib.IsMusicStreamPlaying(_music))
        {
            Stop();
        }

        Raylib.PlayMusicStream(_music);
        if (!Musics.Contains(this))
            Musics.Add(this);
        return this;
    }

    public Music Stop()
    {
        Paused = false;
        Raylib.StopMusicStream(_music);
        return this;
    }

    public Music Pause()
    {
        if (!Raylib.IsMusicStreamPlaying(_music))
            return this;
        Paused = true;
        Raylib.PauseMusicStream(_music);
        return this;
    }

    public Music Resume()
    {
        if (!Paused)
            return this;
        Paused = false;
        Raylib.ResumeMusicStream(_music);
        if (!Musics.Contains(this))
            Musics.Add(this);
        return this;
    }

    public Music Seek(TimeSpan time)
    {
        Raylib.SeekMusicStream(_music, (float)time.TotalSeconds);
        return this;
    }

    internal static void UpdateAll()
    {
        for (var i = Musics.Count - 1; i >= 0; i--)
        {
            var music = Musics[i];
            Raylib.UpdateMusicStream(music._music);
            if (!Raylib.IsMusicStreamPlaying(music._music))
                Musics.RemoveAt(i);
        }
    }

    ~Music()
    {
        Game.Defer(() =>
        {
            Raylib.UnloadMusicStream(_music);
            Marshal.FreeHGlobal(_buffer);
        });
    }
}

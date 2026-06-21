using Raylib_cs;
using Vigilance.Core;

namespace Vigilance.Drawing;

public sealed class VertexArray : IDisposable
{
    public VertexArray()
    {
        Game.ThrowIfNotRunning();
        Id = Rlgl.LoadVertexArray();
    }

    public uint Id { get; } = 0;

    public bool IsValid => Id != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        if (Id != 0)
            Game.Defer(() => Rlgl.UnloadVertexArray(Id));
    }

    ~VertexArray()
    {
        ReleaseUnmanagedResources();
    }
}

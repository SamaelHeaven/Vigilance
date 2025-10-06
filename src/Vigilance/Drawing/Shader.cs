using System.Numerics;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe class Shader
{
    private static readonly string VertexHeader;
    private static readonly string FragmentHeader;
    private readonly Dictionary<string, int> _locations = new();
    internal readonly Raylib_cs.BleedingEdge.Shader RShader;

    static Shader()
    {
        VertexHeader = Game.Platform switch
        {
            Platform.Desktop => "#version 120\n",
            _ => "",
        };
        FragmentHeader = Game.Platform switch
        {
            Platform.Web => "precision mediump float;\n",
            Platform.Desktop => "#version 120\n",
            _ => "",
        };
    }

    internal Shader(Raylib_cs.BleedingEdge.Shader shader)
    {
        RShader = shader;
    }

    public Shader(string? vertex = null, string? fragment = null, bool raw = false)
    {
        Game.EnsureRunning();
        if (vertex == "")
            throw new ArgumentException("Vertex shader cannot be empty.", nameof(vertex));
        if (fragment == "")
            throw new ArgumentException("Fragment shader cannot be empty.", nameof(fragment));
        RShader = Raylib.LoadShaderFromMemory(
            raw ? vertex!
                : vertex is null ? null!
                : $"{VertexHeader}{vertex}",
            raw ? fragment!
                : fragment is null ? null!
                : $"{FragmentHeader}{fragment}"
        );
    }

    public uint Id => RShader.Id;

    public void SetFloat(string uniform, float value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Float);
    }

    public void SetFloatSpan(string uniform, ReadOnlySpan<float> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Float);
    }

    public void SetVec2(string uniform, Vector2 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec2);
    }

    public void SetVec2Span(string uniform, ReadOnlySpan<Vector2> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec2);
    }

    public void SetVec3(string uniform, (float X, float Y, float Z) value)
    {
        SetVec3(uniform, new Vector3(value.X, value.Y, value.Z));
    }

    public void SetVec3(string uniform, Vector3 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec3);
    }

    public void SetVec3Span(string uniform, ReadOnlySpan<(float X, float Y, float Z)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec3);
    }

    public void SetVec3Span(string uniform, ReadOnlySpan<Vector3> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec3);
    }

    public void SetVec4(string uniform, (float X, float Y, float Z, float W) value)
    {
        SetVec4(uniform, new Vector4(value.X, value.Y, value.Z, value.W));
    }

    public void SetVec4(string uniform, Vector4 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec4);
    }

    public void SetVec4Span(string uniform, ReadOnlySpan<(float X, float Y, float Z, float W)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec4);
    }

    public void SetVec4Span(string uniform, ReadOnlySpan<Vector4> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec4);
    }

    public void SetColor(string uniform, Color value)
    {
        SetVec4(uniform, value.Normalize());
    }

    public void SetColorSpan(string uniform, ReadOnlySpan<Color> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Vec4);
    }

    public void SetInt(string uniform, int value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Int);
    }

    public void SetIntSpan(string uniform, ReadOnlySpan<int> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.Int);
    }

    public void SetIVec2(string uniform, (int X, int Y) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec2);
    }

    public void SetIVec2Span(string uniform, ReadOnlySpan<(int X, int Y)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.IVec2);
    }

    public void SetIVec3(string uniform, (int X, int Y, int Z) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec3);
    }

    public void SetIVec3Span(string uniform, ReadOnlySpan<(int X, int Y, int Z)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.IVec3);
    }

    public void SetIVec4(string uniform, (int X, int Y, int Z, int W) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec4);
    }

    public void SetIVec4Span(string uniform, ReadOnlySpan<(int X, int Y, int Z, int W)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.IVec4);
    }

    public void SetUInt(string uniform, uint value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UInt);
    }

    public void SetUIntSpan(string uniform, ReadOnlySpan<uint> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.UInt);
    }

    public void SetUIVec2(string uniform, (uint X, uint Y) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec2);
    }

    public void SetUIVec2Span(string uniform, ReadOnlySpan<(uint X, uint Y)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.UIVec2);
    }

    public void SetUIVec3(string uniform, (uint X, uint Y, uint Z) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec3);
    }

    public void SetUIVec3Span(string uniform, ReadOnlySpan<(uint X, uint Y, uint Z)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.UIVec3);
    }

    public void SetUIVec4(string uniform, (uint X, uint Y, uint Z, uint W) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec4);
    }

    public void SetUIVec4Span(string uniform, ReadOnlySpan<(uint X, uint Y, uint Z, uint W)> values)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), values, ShaderUniformDataType.UIVec4);
    }

    public void SetMatrix(string uniform, Matrix4x4 value)
    {
        Raylib.SetShaderValueMatrix(RShader, GetLocation(uniform), value);
    }

    public void SetTexture(string uniform, Texture texture)
    {
        Raylib.SetShaderValueTexture(RShader, GetLocation(uniform), texture.Texture2D);
    }

    private int GetLocation(string uniform)
    {
        if (_locations.TryGetValue(uniform, out var location))
            return location;
        location = Raylib.GetShaderLocation(RShader, uniform);
        _locations.Add(uniform, location);
        return location;
    }

    ~Shader()
    {
        Raylib.UnloadShader(RShader);
    }
}

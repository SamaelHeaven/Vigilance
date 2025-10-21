using System.Numerics;
using System.Text.RegularExpressions;
using Raylib_cs.BleedingEdge;
using Vigilance.Core;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe partial class Shader : IDisposable
{
    private static readonly Dictionary<string, string> _vertexVersions = new()
    {
        { "vigilance_100", Platform.Web.IsCurrent ? "\n" : "#version 120\n" },
        { "vigilance_300", Platform.Web.IsCurrent ? "#version 300 es\n" : "#version 300\n" },
    };

    private static readonly Dictionary<string, string> _fragmentVersions = new()
    {
        { "vigilance_100", Platform.Web.IsCurrent ? "precision mediump float;\n" : "#version 120\n" },
        { "vigilance_300", Platform.Web.IsCurrent ? "#version 300 es\n" : "#version 300\n" },
    };

    private readonly Dictionary<string, int> _locations = new();
    internal Raylib_cs.BleedingEdge.Shader RShader;

    internal Shader(Raylib_cs.BleedingEdge.Shader shader)
    {
        RShader = shader;
    }

    public Shader(string? vertex = null, string? fragment = null)
    {
        Game.EnsureRunning();
        RShader = Raylib.LoadShaderFromMemory(
            FormatShader(vertex, _vertexVersions),
            FormatShader(fragment, _fragmentVersions)
        );
    }

    public uint Id => RShader.Id;

    public bool IsValid => RShader.Id != 0;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
        RShader = default;
    }

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

    private void ReleaseUnmanagedResources()
    {
        Raylib.UnloadShader(RShader);
    }

    private static string FormatShader(string? shader, Dictionary<string, string> versions)
    {
        if (shader is null)
            return null!;
        return VersionRegex()
            .Replace(
                shader,
                match =>
                {
                    var key = match.Groups[1].Value.ToLower();
                    return versions.TryGetValue(key, out var replacement) ? replacement : match.Value;
                }
            );
    }

    ~Shader()
    {
        Game.Defer(ReleaseUnmanagedResources);
    }

    [GeneratedRegex(@"^\s*#version\s+(\S+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex VersionRegex();
}

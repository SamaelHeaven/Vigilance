using System.Numerics;
using System.Text.RegularExpressions;
using Raylib_cs;
using Vigilance.Collections;
using Vigilance.Core;
using Vigilance.Math;
using Vector2 = Vigilance.Math.Vector2;

namespace Vigilance.Drawing;

public sealed unsafe partial class Shader : IDisposable
{
    private static readonly ValueDictionary<string, string> _vertexVersions = new()
    {
        { "vigilance_100", Platform.Web.IsCurrent ? "\n" : "#version 120\n" },
        { "vigilance_300", Platform.Web.IsCurrent ? "#version 300 es\n" : "#version 330 core\n" },
    };

    private static readonly ValueDictionary<string, string> _fragmentVersions = new()
    {
        { "vigilance_100", Platform.Web.IsCurrent ? "precision mediump float;\n" : "#version 120\n" },
        {
            "vigilance_300",
            Platform.Web.IsCurrent ? "#version 300 es\nprecision mediump float;\n" : "#version 330 core\n"
        },
    };

    private ValueDictionary<string, int> _locations = [];
    internal Raylib_cs.Shader RShader;

    internal Shader(in Raylib_cs.Shader shader)
    {
        RShader = shader;
    }

    public Shader(string? vertex = null, string? fragment = null)
    {
        Game.ThrowIfNotRunning();
        RShader = Raylib.LoadShaderFromMemory(
            FormatShader(vertex, _vertexVersions),
            FormatShader(fragment, _fragmentVersions)
        );
    }

    public static Shader Default
    {
        get
        {
            Game.ThrowIfNotRunning();
            return field ??= new Shader(
                new Raylib_cs.Shader { Id = Rlgl.GetShaderIdDefault(), Locs = Rlgl.GetShaderLocsDefault() }
            );
        }
    }

    public uint Id => RShader.Id;

    public bool IsDefault => Id == Default.Id;

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

    public void SetFloatSpan(string uniform, in ReadOnlySpan<float> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Float, values.Length);
        }
    }

    public void SetVec2(string uniform, Vector2 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec2);
    }

    public void SetVec2Span(string uniform, in ReadOnlySpan<Vector2> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec2, values.Length);
        }
    }

    public void SetVec3(string uniform, (float X, float Y, float Z) value)
    {
        SetVec3(uniform, new Vector3(value.X, value.Y, value.Z));
    }

    public void SetVec3(string uniform, Vector3 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec3);
    }

    public void SetVec3Span(string uniform, in ReadOnlySpan<(float X, float Y, float Z)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec3, values.Length);
        }
    }

    public void SetVec3Span(string uniform, in ReadOnlySpan<Vector3> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec3, values.Length);
        }
    }

    public void SetVec4(string uniform, (float X, float Y, float Z, float W) value)
    {
        SetVec4(uniform, new Vector4(value.X, value.Y, value.Z, value.W));
    }

    public void SetVec4(string uniform, Vector4 value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Vec4);
    }

    public void SetVec4Span(string uniform, in ReadOnlySpan<(float X, float Y, float Z, float W)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec4, values.Length);
        }
    }

    public void SetVec4Span(string uniform, in ReadOnlySpan<Vector4> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec4, values.Length);
        }
    }

    public void SetColor(string uniform, Color value)
    {
        SetVec4(uniform, value.Normalize());
    }

    public void SetColorSpan(string uniform, in ReadOnlySpan<Color> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Vec4, values.Length);
        }
    }

    public void SetInt(string uniform, int value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.Int);
    }

    public void SetIntSpan(string uniform, in ReadOnlySpan<int> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.Int, values.Length);
        }
    }

    public void SetIVec2(string uniform, (int X, int Y) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec2);
    }

    public void SetIVec2Span(string uniform, in ReadOnlySpan<(int X, int Y)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.IVec2, values.Length);
        }
    }

    public void SetIVec3(string uniform, (int X, int Y, int Z) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec3);
    }

    public void SetIVec3Span(string uniform, in ReadOnlySpan<(int X, int Y, int Z)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.IVec3, values.Length);
        }
    }

    public void SetIVec4(string uniform, (int X, int Y, int Z, int W) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.IVec4);
    }

    public void SetIVec4Span(string uniform, in ReadOnlySpan<(int X, int Y, int Z, int W)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.IVec4, values.Length);
        }
    }

    public void SetUInt(string uniform, uint value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UInt);
    }

    public void SetUIntSpan(string uniform, in ReadOnlySpan<uint> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.UInt, values.Length);
        }
    }

    public void SetUIVec2(string uniform, (uint X, uint Y) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec2);
    }

    public void SetUIVec2Span(string uniform, in ReadOnlySpan<(uint X, uint Y)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.UIVec2, values.Length);
        }
    }

    public void SetUIVec3(string uniform, (uint X, uint Y, uint Z) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec3);
    }

    public void SetUIVec3Span(string uniform, in ReadOnlySpan<(uint X, uint Y, uint Z)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.UIVec3, values.Length);
        }
    }

    public void SetUIVec4(string uniform, (uint X, uint Y, uint Z, uint W) value)
    {
        Raylib.SetShaderValue(RShader, GetLocation(uniform), &value, ShaderUniformDataType.UIVec4);
    }

    public void SetUIVec4Span(string uniform, in ReadOnlySpan<(uint X, uint Y, uint Z, uint W)> values)
    {
        fixed (void* ptr = values)
        {
            Raylib.SetShaderValueV(RShader, GetLocation(uniform), ptr, ShaderUniformDataType.UIVec4, values.Length);
        }
    }

    public void SetMatrix(string uniform, in Matrix3x2 value)
    {
        Raylib.SetShaderValueMatrix(RShader, GetLocation(uniform), value.ToMatrix4x4());
    }

    public void SetMatrix(string uniform, in Matrix4x4 value)
    {
        Raylib.SetShaderValueMatrix(RShader, GetLocation(uniform), value);
    }

    public void SetTexture(string uniform, Texture texture)
    {
        Raylib.SetShaderValueTexture(RShader, GetLocation(uniform), texture.Texture2D);
    }

    public int GetLocation(string uniform)
    {
        ref var location = ref _locations.GetValueRefOrAddDefault(uniform, out var exists);
        if (!exists)
            location = Raylib.GetShaderLocation(RShader, uniform);
        return location;
    }

    private void ReleaseUnmanagedResources()
    {
        if (Id == Default.Id)
            return;
        Raylib.UnloadShader(RShader);
    }

    private static string FormatShader(string? shader, ValueDictionary<string, string> versions)
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
                },
                1
            );
    }

    ~Shader()
    {
        Game.Defer(ReleaseUnmanagedResources);
    }

    [GeneratedRegex(@"^\s*#version\s+(\S+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex VersionRegex();

    public static class Vertex;

    public static class Fragment;
}

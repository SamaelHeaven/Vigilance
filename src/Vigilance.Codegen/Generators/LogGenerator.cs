using System.Text;
using Microsoft.CodeAnalysis;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class LogGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.Logging;

            public static partial class Log
            {

            """
        );
        Log(sb, "Invoke");
        Log(sb, "Invoke", true);
        Log(sb, "Trace");
        Log(sb, "Debug");
        Log(sb, "Info");
        Log(sb, "Warning");
        Log(sb, "Error");
        Log(sb, "Fatal");
        sb.AppendLine("}");
    }

    private static void Log(StringBuilder sb, string type, bool levelArg = false)
    {
        for (var i = 1; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var args = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n} t{n}"));
            var interpolation = string.Join(" ", Enumerable.Range(0, i + 1).Select(n => "{t" + n + "}"));
            sb.AppendLine(
                $$"""
                    public static void {{type}}<{{typeParams}}>({{(levelArg ? "LogLevel level, " : "")}}{{args}})
                    {
                        {{type}}({{(levelArg ? "level, " : "")}}$"{{interpolation}}");
                    }
                    
                """
            );
        }
    }
}

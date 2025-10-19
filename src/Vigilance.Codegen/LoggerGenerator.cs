using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen;

[Generator]
public sealed class LoggerGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            namespace Vigilance.Logging;

            public static partial class Logger
            {

            """
        );
        Log(sb, "Log");
        Log(sb, "Log", true);
        Log(sb, "Debug");
        Log(sb, "Info");
        Log(sb, "Warning");
        Log(sb, "Error");
        Log(sb, "Fatal");
        sb.AppendLine("}");
        context.AddSource("Logger.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Log(StringBuilder sb, string type, bool levelArg = false)
    {
        for (var i = 1; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var args = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n} t{n}"));
            var interpolation = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => "{t" + n + "}"));
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

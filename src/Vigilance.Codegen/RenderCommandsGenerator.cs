using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen;

[Generator]
public sealed class RenderCommandsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            using Vigilance.Core;

            namespace Vigilance.Drawing;

            public readonly ref partial struct RenderCommands 
            {

            """
        );
        AddRange(sb, false, false);
        AddRange(sb, false, true);
        AddRange(sb, true, false);
        AddRange(sb, true, true);
        sb.AppendLine("}");
        context.AddSource("RenderCommand.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    public static void AddRange(StringBuilder sb, bool entryEnumerable, bool context)
    {
        for (var i = 1; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var items = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"entry.Item{n + 2}"));
            sb.AppendLine(
                $$"""
                    public void AddRange<{{(context ? "TContext, " : "")}}{{typeParams}}>({{(
                        context ? "TContext context, " : ""
                    )}}{{(entryEnumerable ? $"Scene.EntryEnumerable<{typeParams}>" : $"IEnumerable<(Entity, {typeParams})>")}} entries, Action<Entity, {{(
                    context ? "TContext, " : ""
                )}}({{typeParams}})> action)
                    {
                        foreach (var entry in entries)
                            Add(entry.Item1, {{(context ? "context, " : "")}}({{items}}), action);
                    }
                    
                """
            );
        }
    }
}

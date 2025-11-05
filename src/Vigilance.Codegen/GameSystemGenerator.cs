using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen;

[Generator]
public sealed class GameSystemGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            #pragma warning disable CS9084

            namespace Vigilance.Core;

            public abstract partial class GameSystem
            {

            """
        );
        Entities(sb);
        Components(sb);
        Entries(sb);
        sb.AppendLine("}");
        context.AddSource("GameSystem.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Entities(StringBuilder sb)
    {
        sb.BeginRegion("Entities");
        sb.AppendLine(QueryIterator("Entity", "GetEntities", visibility: "private"));
        sb.EndRegion();
    }

    private static void Components(StringBuilder sb)
    {
        sb.BeginRegion("Components");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Component", "Components", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Entries(StringBuilder sb)
    {
        sb.BeginRegion("Entries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Entry", "Entries", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static string QueryIterator(
        string name,
        string methodName,
        string typeParams = "",
        string visibility = "public"
    )
    {
        return $$"""
                {{visibility}} Scene.{{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}() {
                    return Scene.{{methodName}}{{typeParams}}().WithDisabled(WithDisabled).Deferred(Deferred);
                }

            """;
    }
}

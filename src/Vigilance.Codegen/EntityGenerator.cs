using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen;

[Generator]
public sealed class EntityGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            namespace Vigilance.Core;

            public readonly partial record struct Entity
            {

            """
        );
        Has(sb);
        TryGet(sb);
        sb.AppendLine("}");
        context.AddSource("Entity.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Has(StringBuilder sb)
    {
        sb.Region("Has");
        for (var i = 0; i < 16; i++)
        {
            var genericParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var hasChecks = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"_entity.Has<T{n}>()"));
            sb.AppendLine(
                $$"""
                    public bool Has<{{genericParams}}>()
                    {
                        return {{hasChecks}};
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void TryGet(StringBuilder sb)
    {
        sb.Region("TryGet");
        for (var i = 1; i < 16; i++)
        {
            var genericParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var outParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"out T{n} t{n}"));
            var defaultAssign = string.Join("\n        ", Enumerable.Range(0, i + 1).Select(n => $"t{n} = default!;"));
            var tryCalls = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"TryGet(out t{n})"));
            sb.AppendLine(
                $$"""
                    public bool TryGet<{{genericParams}}>({{outParams}})
                    {
                        {{defaultAssign}}
                        return {{tryCalls}};
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }
}

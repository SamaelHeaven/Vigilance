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
        sb.BeginRegion("Has");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var hasChecks = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"flecsEntity.Has<T{n}>()"));
            sb.AppendLine(
                $$"""
                    public bool Has<{{typeParams}}>()
                    {
                        EnsureValid();
                        var flecsEntity = FlecsEntity;
                        return {{hasChecks}};
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void TryGet(StringBuilder sb)
    {
        sb.BeginRegion("TryGet");
        for (var i = 1; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var outParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"out T{n} t{n}"));
            var skipInits = string.Join(
                "\n        ",
                Enumerable.Range(0, i + 1).Select(n => $"System.Runtime.CompilerServices.Unsafe.SkipInit(out t{n});")
            );
            var tryGets = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"TryGet(out t{n})"));
            sb.AppendLine(
                $$"""
                    public bool TryGet<{{typeParams}}>({{outParams}})
                    {
                        EnsureValid();
                        {{skipInits}}
                        return {{tryGets}};
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }
}

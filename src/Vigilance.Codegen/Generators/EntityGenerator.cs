using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class EntityGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            namespace Vigilance.Core;

            public readonly partial record struct Entity
            {

            """
        );
        Has(sb);
        TryGet(sb);
        TryGetRef(sb);
        sb.AppendLine("}");
        sb.AppendLine(
            """
             
            public static partial class EntityExtensions
            {

            """
        );
        sb.TraverserExtensions("Entity", "Entity.Traverser", "bool deferred = true", "deferred");
        sb.AppendLine("}");
    }

    private static void Has(StringBuilder sb)
    {
        sb.BeginRegion("Has");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var hasChecks = string.Join(
                " && ",
                Enumerable.Range(0, i + 1).Select(n => $"Scene.Table<T{n}>().Has(this)")
            );
            sb.AppendLine(
                $$"""
                    public bool Has<{{typeParams}}>()
                    {
                        AssertValid();
                        return Scene is null ? false : ({{hasChecks}});
                    }
                    
                """
            );
        }

        for (var i = 0; i < 16; i++)
        {
            var tableParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"Table table{n}"));
            var hasChecks = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"table{n}.Has(this)"));
            sb.AppendLine(
                $$"""
                    public bool Has({{tableParams}})
                    {
                        AssertValid();
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
            var tryGets = string.Join(
                " && ",
                Enumerable.Range(0, i + 1).Select(n => $"Scene.Table<T{n}>().TryGet(this, out t{n})")
            );
            sb.AppendLine(
                $$"""
                    public bool TryGet<{{typeParams}}>({{outParams}})
                    {
                        AssertValid();
                        {{skipInits}}
                        return Scene is null ? false : ({{tryGets}});
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void TryGetRef(StringBuilder sb)
    {
        sb.BeginRegion("TryGetRef");
        for (var i = 1; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var outParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"out ComponentRef<T{n}> t{n}"));
            var skipInits = string.Join(
                "\n        ",
                Enumerable.Range(0, i + 1).Select(n => $"System.Runtime.CompilerServices.Unsafe.SkipInit(out t{n});")
            );
            var tryGets = string.Join(
                " && ",
                Enumerable.Range(0, i + 1).Select(n => $"Scene.Table<T{n}>().TryGetRef(this, out t{n})")
            );
            sb.AppendLine(
                $$"""
                    public bool TryGetRef<{{typeParams}}>({{outParams}})
                    {
                        AssertValid();
                        {{skipInits}}
                        return Scene is null ? false : ({{tryGets}});
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }
}

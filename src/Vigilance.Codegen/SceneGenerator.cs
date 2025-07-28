using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Vigilance.Codegen;

[Generator]
public sealed class SceneGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            namespace Vigilance.Core;

            public sealed partial class Scene
            {

            """
        );
        Each(sb);
        OrderedEach(sb);
        ComponentEnumerator(sb);
        EntryEnumerator(sb);
        sb.AppendLine("}");
        context.AddSource("Scene.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Each(StringBuilder sb)
    {
        sb.Region("Each");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var args = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"ref T{n} t{n}"));
            var invokeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"t{n}"));
            sb.AppendLine(
                $$"""
                    public void Each<{{typeParams}}>(System.Action<{{typeParams}}> action)
                    {
                        EnsureInitialized();
                        DeferBegin();
                        _world.Each(({{args}}) => action.Invoke({{invokeParams}}));
                        DeferEnd();
                    }
                    
                """
            );
            if (i == 15)
                break;
            sb.AppendLine(
                $$"""
                    public void Each<{{typeParams}}>(System.Action<Entity, {{typeParams}}> action)
                    {
                        EnsureInitialized();
                        DeferBegin();
                        _world.Each((Flecs.NET.Core.Entity entity, {{args}}) => action.Invoke(new Entity(entity, this), {{invokeParams}}));
                        DeferEnd();
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void OrderedEach(StringBuilder sb)
    {
        sb.Region("OrderedEach");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var invokeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"entity.Get<T{n}>()"));
            var hasChecks = string.Join(" && ", Enumerable.Range(0, i + 1).Select(n => $"entity.Has<T{n}>()"));
            sb.AppendLine(
                $$"""
                    public void OrderedEach<{{typeParams}}>(System.Action<{{typeParams}}> action)
                    {
                        EnsureInitialized();
                        DeferBegin();
                        _orderedQuery.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) =>
                        {
                            if ({{hasChecks}})
                                action.Invoke({{invokeParams}});
                        });
                        DeferEnd();
                    }
                    
                """
            );
            if (i == 15)
                break;
            sb.AppendLine(
                $$"""
                    public void OrderedEach<{{typeParams}}>(System.Action<Entity, {{typeParams}}> action)
                    {
                        EnsureInitialized();
                        DeferBegin();
                        _orderedQuery.Each((Flecs.NET.Core.Entity entity, ref ZIndex _) =>
                        {
                            if ({{hasChecks}})
                                action.Invoke(new Entity(entity, this), {{invokeParams}});
                        });
                        DeferEnd();
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void ComponentEnumerator(StringBuilder sb)
    {
        sb.Region("ComponentEnumerator");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var tupleType = i == 0 ? "T0" : $"({typeParams})";
            var getFields =
                i == 0
                    ? "GetField<T0>(0)"
                    : "(" + string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"GetField<T{n}>({n})")) + ")";
            var queryArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var className = $"ComponentEnumerator<{typeParams}>";
            sb.AppendLine(
                $$"""
                    private class {{className}} : QueryEnumerator<{{tupleType}}, Flecs.NET.Core.Query<{{queryArgs}}>>
                    {
                        public ComponentEnumerator(Scene scene)
                            : base(scene) { }

                        public override {{tupleType}} Current => {{getFields}};

                        protected override Flecs.NET.Core.Query<{{queryArgs}}> Query()
                        {
                            return Scene._world.QueryBuilder<{{queryArgs}}>().Build();
                        }
                    }
                    
                """
            );
            sb.AppendLine(
                $$"""
                    public System.Collections.Generic.IEnumerable<{{tupleType}}> Components<{{typeParams}}>()
                    {
                        EnsureInitialized();
                        return new {{className}}(this);
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void EntryEnumerator(StringBuilder sb)
    {
        sb.Region("EntryEnumerator");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var tupleTypes = $"(Entity, {typeParams})";
            var getFields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"GetField<T{n}>({n})"));
            var currentTuple = $"(CurrentEntity, {getFields})";
            var className = $"EntryEnumerator<{typeParams}>";
            sb.AppendLine(
                $$"""
                    private class {{className}} : QueryEnumerator<{{tupleTypes}}, Flecs.NET.Core.Query<{{typeParams}}>>
                    {
                        public EntryEnumerator(Scene scene)
                            : base(scene) { }

                        public override {{tupleTypes}} Current => {{currentTuple}};

                        protected override Flecs.NET.Core.Query<{{typeParams}}> Query()
                        {
                            return Scene._world.QueryBuilder<{{typeParams}}>().Build();
                        }
                    }
                    
                """
            );
            sb.AppendLine(
                $$"""
                    public System.Collections.Generic.IEnumerable<{{tupleTypes}}> Entries<{{typeParams}}>()
                    {
                        EnsureInitialized();
                        return new {{className}}(this);
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }
}

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
            #pragma warning disable CS9084

            namespace Vigilance.Core;

            public sealed unsafe partial class Scene
            {

            """
        );
        Entities(sb);
        Components(sb);
        Entries(sb);
        sb.AppendLine("}");
        context.AddSource("Scene.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Entities(StringBuilder sb)
    {
        sb.BeginRegion("Entities");
        sb.AppendLine(
            QueryIterator("Entity", "Entity", "ZIndex", "CurrentEntity", "GetEntities", visibility: "internal")
        );
        sb.EndRegion();
    }

    private static void Components(StringBuilder sb)
    {
        sb.BeginRegion("Components");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type = i == 0 ? "T0" : $"({typeParams})";
            var current =
                i == 0
                    ? "GetField<T0>(0)"
                    : "(" + string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"GetField<T{n}>({n})")) + ")";
            var queryArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            sb.AppendLine(QueryIterator("Component", type, queryArgs, current, "Components", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void Entries(StringBuilder sb)
    {
        sb.BeginRegion("Entries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var type = $"(Entity, {typeParams})";
            var getFields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"GetField<T{n}>({n})"));
            var current = $"(CurrentEntity, {getFields})";
            sb.AppendLine(QueryIterator("Entry", type, typeParams, current, "Entries", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static string QueryIterator(
        string name,
        string type,
        string queryTypeParams,
        string current,
        string methodName,
        string typeParams = "",
        string visibility = "public"
    )
    {
        return $$"""
                public struct {{name}}Enumerable{{typeParams}} : IStructEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>
                {
                    private readonly Scene _scene;
                    private WithDisabled _withDisabled;
                    private bool _defer;
                
                    internal {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                        _withDisabled = Vigilance.Core.WithDisabled.No;
                        _defer = Ecs.DefaultDefer;
                    }
                    
                    public {{name}}Enumerator{{typeParams}} GetEnumerator()
                    {
                        return new {{name}}Enumerator{{typeParams}}(_scene, _withDisabled, _defer);
                    }
                    
                    public ZLinq.ValueEnumerable<StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>, {{type}}> AsValueEnumerable()
                    {
                        return new StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>(GetEnumerator());
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(WithDisabled value = Vigilance.Core.WithDisabled.Yes) {
                        _withDisabled = value;
                        return ref this;
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} Defer(bool defer = true) {
                        _defer = defer;
                        return ref this;
                    }
                }
                
                public unsafe struct {{name}}Enumerator{{typeParams}} : IStructEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    private readonly WithDisabled _withDisabled;
                    private readonly bool _defer;
                    private Flecs.NET.Core.Query<{{queryTypeParams}}>? _query;
                    private Flecs.NET.Bindings.flecs.ecs_iter_t _iter;
                    private int _index;
                    
                    internal {{name}}Enumerator(Scene scene, WithDisabled withDisabled, bool defer)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _defer = defer;
                        Reset();
                    }
                
                    private Entity CurrentEntity
                    {
                        get
                        {
                            if (!_query.HasValue)
                                return Core.Entity.Null;
                            return new Entity(_iter.entities[_index], _scene);
                        }
                    }
                    
                    private TField GetField<TField>(byte index)
                    {
                        fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                        {
                            var ptr = Flecs.NET.Bindings.flecs.ecs_field_w_size(iter, Flecs.NET.Core.Type<TField>.Size, index);
                            if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<TField>())
            #pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                                return ((TField*)ptr)[_index];
            #pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(*&((nint*)ptr)[_index]);
                            var box = (System.Runtime.CompilerServices.StrongBox<TField>)handle.Target!;
                            return box.Value!;
                        }
                    }

                    public bool MoveNext()
                    {
                        if (_index < _iter.count)
                        {
                            _index++;
                            if (_index < _iter.count)
                                return true;
                        }

                        _index = 0;
                        fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                        {
                            return Flecs.NET.Bindings.flecs.ecs_query_next(iter);
                        }
                    }

                    public void Reset()
                    {
                        Dispose();
                        if (_defer)
                            _scene.BeginDefer();
                        var queryBuilder = _scene.World.QueryBuilder<{{queryTypeParams}}>().CacheKind(Flecs.NET.Bindings.flecs.ecs_query_cache_kind_t.EcsQueryCacheNone);
                        queryBuilder = _withDisabled switch
                        {
                           WithDisabled.Only => queryBuilder.With(Flecs.NET.Core.Ecs.Disabled),
                           WithDisabled.Yes => queryBuilder.With(Flecs.NET.Core.Ecs.Disabled).Optional(),
                           _ => queryBuilder
                        };
                        
                        var query = queryBuilder.Build();
                        _query = query;
                        _iter = query.GetIter();
                        _index = 0;
                        fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                        {
                            Flecs.NET.Core.Ecs.TableLock(iter);
                        }
                    }

                    public {{type}} Current => {{current}};

                    public void Dispose()
                    {
                        if (!_query.HasValue)
                            return;
                        fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                        {
                            Flecs.NET.Core.Ecs.TableUnlock(iter);
                        }

                        if (_defer)
                            _scene.EndDefer();
                        _query.Value.Dispose();
                        _query = null;
                        _iter = default;
                        _index = 0;
                    }
                }
                
                {{visibility}} {{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}() {
                    EnsureInitialized();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }
                
            """;
    }
}

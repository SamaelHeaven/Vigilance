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
        Entities(sb);
        Components(sb);
        Entries(sb);
        sb.AppendLine("}");
        context.AddSource("Scene.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void Entities(StringBuilder sb)
    {
        sb.Region("Entities");
        sb.AppendLine(QueryIterator("Entity", "Entity", "ZIndex", "CurrentEntity", "GetEntities"));
        sb.AppendLine(
            QueryIterator(
                "OrderedEntity",
                "Entity",
                "ZIndex",
                "CurrentEntity",
                "GetOrderedEntities",
                query: "_scene._orderedQuery"
            )
        );
        sb.EndRegion();
    }

    private static void Components(StringBuilder sb)
    {
        sb.Region("Components");
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
        sb.Region("Entries");
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
        string query = ""
    )
    {
        return $$"""
                public readonly struct {{name}}Enumerable{{typeParams}}
                {
                    private readonly Scene _scene;
                
                    internal {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                    }
                    
                    public {{name}}Enumerator{{typeParams}} GetEnumerator()
                    {
                        return new {{name}}Enumerator{{typeParams}}(_scene);
                    }
                    
                    public List<{{type}}> ToList()
                    {
                        var list = new List<{{type}}>();
                        foreach (var item in this)
                            list.Add(item);
                        return list;
                    }
                }
                
                public unsafe ref struct {{name}}Enumerator{{typeParams}} {
                    private readonly Scene _scene;
                    private int _index;
                    private Flecs.NET.Bindings.flecs.ecs_iter_t _iter;
                    private Flecs.NET.Core.Query<{{queryTypeParams}}>? _query;
                    
                    internal {{name}}Enumerator(Scene scene)
                    {
                        _scene = scene;
                        Reset();
                    }
                
                    private Entity CurrentEntity
                    {
                        get
                        {
                            if (!_query.HasValue)
                                return Core.Entity.Null;
                            var entity = new Flecs.NET.Core.Entity(_scene._world, _iter.entities[_index]);
                            return new Entity(entity, _scene);
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
                            return Flecs.NET.Utilities.Utils.Bool(Flecs.NET.Bindings.flecs.ecs_query_next(iter));
                        }
                    }

                    public void Reset()
                    {
                        if (!_query.HasValue)
                            Dispose();
                        _scene.DeferBegin();
                        var query = {{(
                            query == "" ? $"_scene._world.QueryBuilder<{queryTypeParams}>().Build()" : query
                        )}};
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

                        _scene.DeferEnd();{{(query == "" ? "\n            _query.Value.Dispose();" : "")}}
                    }
                }
                
                public {{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}() {
                    EnsureInitialized();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }
                
            """;
    }
}

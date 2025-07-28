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
        Entities(sb);
        Components(sb);
        Entries(sb);
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

    private static void Entities(StringBuilder sb)
    {
        sb.Region("Entities");
        sb.AppendLine(QueryEnumerable("EntityEnumerable", "Entity", "Entity", "CurrentEntity", "GetEntities"));
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
            sb.AppendLine(
                QueryEnumerable("ComponentEnumerable", type, queryArgs, current, "Components", $"<{typeParams}>")
            );
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
            sb.AppendLine(QueryEnumerable("EntryEnumerable", type, typeParams, current, "Entries", $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static string QueryEnumerable(
        string name,
        string type,
        string queryTypeParams,
        string current,
        string methodName,
        string typeParams = ""
    )
    {
        return $$"""
                public readonly unsafe struct {{name}}{{typeParams}} : System.Collections.Generic.IEnumerable<{{type}}>
                {
                    public Scene Scene { get; }
                
                    internal {{name}}(Scene scene)
                    {
                        Scene = scene;
                    }
                    
                    public Enumerator GetEnumerator()
                    {
                        return new Enumerator(Scene);
                    }
                    
                    System.Collections.Generic.IEnumerator<{{type}}> System.Collections.Generic.IEnumerable<{{type}}>.GetEnumerator()
                    {
                        return GetEnumerator();
                    }

                    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                    {
                        return GetEnumerator();
                    }
                
                    public struct Enumerator : System.Collections.Generic.IEnumerator<{{type}}>
                    {
                        private int _index;
                        private Flecs.NET.Bindings.flecs.ecs_iter_t _iter;
                        private Flecs.NET.Core.Query<{{queryTypeParams}}>? _query;
                
                        public Scene Scene { get; }
                
                        internal Enumerator(Scene scene)
                        {
                            Scene = scene;
                            Reset();
                        }
                
                        private Entity CurrentEntity
                        {
                            get
                            {
                                if (!_query.HasValue)
                                    return Core.Entity.Null;
                                var entity = new Flecs.NET.Core.Entity(Scene._world, _iter.entities[_index]);
                                return new Entity(entity, Scene);
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
                            if (!_query.HasValue)
                                return false;
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
                            Scene.DeferBegin();
                            var query = Scene._world.QueryBuilder<{{queryTypeParams}}>().Build();
                            _query = query;
                            _iter = query.GetIter();
                            _index = 0;
                            fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                            {
                                Flecs.NET.Core.Ecs.TableLock(iter);
                            }
                        }
                
                        public {{type}} Current => {{current}};
                
                        object System.Collections.IEnumerator.Current => Current;
                
                        public void Dispose()
                        {
                            if (!_query.HasValue)
                                return;
                            fixed (Flecs.NET.Bindings.flecs.ecs_iter_t* iter = &_iter)
                            {
                                Flecs.NET.Core.Ecs.TableUnlock(iter);
                            }
                
                            Scene.DeferEnd();
                            _query.Value.Dispose();
                            _query = null;
                            _iter = default;
                            _index = 0;
                        }
                    }
                }
                
                public {{name}}{{typeParams}} {{methodName}}{{typeParams}}() {
                    EnsureInitialized();
                    return new {{name}}{{typeParams}}(this);
                }
                
            """;
    }
}

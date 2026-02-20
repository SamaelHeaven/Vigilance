using System.Text;
using Microsoft.CodeAnalysis;
using Vigilance.Codegen.Helpers;

namespace Vigilance.Codegen.Generators;

[Generator]
public sealed class SceneGenerator : SourceGenerator
{
    protected override void Generate(StringBuilder sb)
    {
        sb.AppendLine(
            """
            #pragma warning disable CS9084

            namespace Vigilance.Core;

            public sealed unsafe partial class Scene
            {

            """
        );
        Build(sb);
        Entities(sb);
        Components(sb);
        Entries(sb);
        sb.AppendLine("}");
    }

    private static void Build(StringBuilder sb)
    {
        sb.BeginRegion("Build");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var wheres = string.Join(" ", Enumerable.Range(0, i + 1).Select(n => $"where T{n} : IGameSystem, new()"));
            var newArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"new T{n}()"));
            sb.AppendLine(
                $$"""
                    public static Scene Build<{{typeParams}}>(GameSystemsFunc? systems = null)
                        {{wheres}}
                    {
                        return new Scene(() => (systems?.Invoke() ?? []).Concat([{{newArgs}}]));
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void Entities(StringBuilder sb)
    {
        sb.BeginRegion("Entities");
        sb.AppendLine(
            QueryIterator(
                "Entity",
                "Entity",
                "GetEntities",
                "_entity",
                ["ZIndex"],
                visibility: "internal",
                noFields: true
            )
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
                    ? "_field0"
                    : "(" + string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}")) + ")";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(QueryIterator("Component", type, "Components", current, tables, $"<{typeParams}>"));
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
            var getFields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var current = $"(_entity, {getFields})";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(QueryIterator("Entry", type, "Entries", current, tables, $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static string QueryIterator(
        string name,
        string type,
        string methodName,
        string current,
        List<string> tables,
        string typeParams = "",
        string visibility = "public",
        bool noFields = false
    )
    {
        return $$"""
                public struct {{name}}Enumerable{{typeParams}} : Collections.IStructEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>
                {
                    private readonly Scene _scene;
                    private bool _withDisabled;
                    private bool _deferred;
                
                    internal {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                        _deferred = true;
                    }
                    
                    public {{name}}Enumerator{{typeParams}} GetEnumerator()
                    {
                        return new {{name}}Enumerator{{typeParams}}(_scene, _withDisabled, _deferred);
                    }
                    
                    public ZLinq.ValueEnumerable<Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>, {{type}}> AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>(GetEnumerator());
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool value = true) {
                        _withDisabled = value;
                        return ref this;
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} Deferred(bool deferred = true) {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public unsafe struct {{name}}Enumerator{{typeParams}} : Collections.IStructEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _disposed;
                    private int _index;
                    {{(tables.Count > 1 ? "private int _tableIndex; " : "")}}
                    private Entity _entity;
            {{string.Join("\n", tables.Select((t, i) => $"        private Table<{t}> _table{i};"))}}
            {{(noFields ? "" : string.Join("\n", tables.Select((t, i) => $"        private {t} _field{i} = default!;")))}}
                    
                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
            {{string.Join("\n", tables.Select((t, i) => $"            _table{i} = _scene.Table<{t}>();"))}}
                        Reset();
                    }

                    public bool MoveNext()
                    {
                        {{(tables.Count > 1 ? "switch (_tableIndex)\n            " : "")}}{
            {{string.Join("\n", tables.Select((_, i) => $$"""
                                {{(tables.Count > 1 ? $"case {i}:\n                " : "")}}{
                                    TABLE{{i}}:
                                    var newIndex = _index + 1;
                                    if (newIndex >= _table{{i}}.Count) 
                                        return false;
                                    _index = newIndex;
                                    _entity = new Entity(_table{{i}}.Entities[_index], _scene);
                                    if (!_withDisabled && _scene.DisabledTable.Has(_entity))
                                        goto TABLE{{i}};
                {{string.Join("\n", tables.Select((_, j) => j == i ? "" : $"""
                                        ref var field{j} = ref _table{j}.GetRef(_entity).Value;
                                        if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref field{j}))
                                            goto TABLE{i};
                                        _field{j} = field{j};
                    """).Where(str => str != ""))}}
                                    {{(noFields ? "" : $"_field{i} = _table{i}.Components[_index];")}}
                                    return true;
                                }
                """))}}
                        }
                        
            #pragma warning disable CS0162
                        return false;
            #pragma warning restore CS0162
                    }

                    public void Reset()
                    {
                        Dispose();
                        _index = -1;
                        _entity = Core.Entity.Null;
            {{(noFields ? "" : string.Join("\n", tables.Select((_, i) => $"            _field{i} = default!;")))}}{{(tables.Count > 1 ? "            var smallestCount = int.MaxValue;\n" : "")}}
            {{(tables.Count > 1 ? string.Join("\n", tables.Select((_, i) => $$"""
                            if (_table{{i}}.Count < smallestCount)
                            {
                                smallestCount = _table{{i}}.Count;
                                _tableIndex = {{i}};
                            }
                            
                """)) : "")}}
                        if (_deferred)
                            _scene.BeginDefer();
                        _disposed = false;
                    }

                    public {{type}} Current => {{current}};

                    public void Dispose()
                    {
                        if (_disposed)
                            return;
                        if (_deferred)
                            _scene.EndDefer();
                        _disposed = true;
                    }
                }
                
                {{visibility}} {{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}() {
                    ThrowIfNotInitialized();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }
                
            """;
    }
}

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
        AssignableEntities(sb);
        AssignableComponents(sb);
        AssignableEntries(sb);
        RefComponents(sb);
        RefEntries(sb);
        TableEntities(sb);
        TableComponents(sb);
        TableEntries(sb);
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
                        return new Scene(() => {
                            try 
                            {
                                return (systems?.Invoke() ?? []).Concat([{{newArgs}}]);
                            } 
                            catch (System.Reflection.TargetInvocationException e)
                            {
                                if (e.InnerException is null) 
                                    throw;
                                else
                                    throw new Exception(e.InnerException.Message, e.InnerException);
                            }
                        });
                    }
                    
                """
            );
        }

        sb.EndRegion();
    }

    private static void Entities(StringBuilder sb)
    {
        sb.BeginRegion("Entities");
        sb.AppendLine(QueryIterator("Entity", "Entity", "Entities", "_entity", ["ZIndex"], noFields: true));
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(QueryIterator("Entity", "Entity", "Entities", "_entity", tables, $"<{typeParams}>", true));
        }

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
            sb.AppendLine(
                QueryIterator(
                    "Component",
                    type,
                    "Components",
                    current,
                    tables,
                    $"<{typeParams}>",
                    noEntity: true,
                    refName: "RefComponent"
                )
            );
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
            sb.AppendLine(
                QueryIterator("Entry", type, "Entries", current, tables, $"<{typeParams}>", refName: "RefEntry")
            );
        }

        sb.EndRegion();
    }

    private static void RefComponents(StringBuilder sb)
    {
        sb.BeginRegion("RefComponents");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var componentRefs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"ComponentRef<T{n}>"));
            var type = i == 0 ? "ComponentRef<T0>" : $"RefTuple<{componentRefs}>";
            var fields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var current = i == 0 ? "_field0" : $"new RefTuple<{componentRefs}>({fields})";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(RefQueryIterator("RefComponent", type, current, tables, $"<{typeParams}>", true));
        }

        sb.EndRegion();
    }

    private static void RefEntries(StringBuilder sb)
    {
        sb.BeginRegion("RefEntries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var componentRefs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"ComponentRef<T{n}>"));
            var tupleTypes = $"Entity, {componentRefs}";
            var fields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var type = $"RefTuple<{tupleTypes}>";
            var current = $"new RefTuple<{tupleTypes}>(_entity, {fields})";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(RefQueryIterator("RefEntry", type, current, tables, $"<{typeParams}>"));
        }

        sb.EndRegion();
    }

    private static void AssignableEntities(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntities");
        sb.AppendLine(
            AssignableQueryIterator(
                "AssignableEntity",
                "Entity",
                "AssignableEntities",
                "_entity",
                "<T0>",
                1,
                "TableEntity1Enumerator",
                "_items",
                "Entities",
                "private Entity _entity;",
                "_entity = Core.Entity.Null;",
                "_entity = _items.Current;"
            )
        );

        sb.EndRegion();
    }

    private static void AssignableComponents(StringBuilder sb)
    {
        sb.BeginRegion("AssignableComponents");
        sb.AppendLine(
            AssignableQueryIterator(
                "AssignableComponent",
                "T0",
                "AssignableComponents",
                "(T0)_component",
                "<T0>",
                1,
                "TableComponent1Enumerator",
                "_items",
                "Components",
                "private object _component;",
                "_component = default!;",
                "_component = _items.Current;"
            )
        );

        sb.EndRegion();
    }

    private static void AssignableEntries(StringBuilder sb)
    {
        sb.BeginRegion("AssignableEntries");
        sb.AppendLine(
            AssignableQueryIterator(
                "AssignableEntries",
                "(Entity, T0)",
                "AssignableEntries",
                "(_entity, (T0)_component)",
                "<T0>",
                1,
                "TableEntry1Enumerator",
                "_items",
                "Entries",
                "private Entity _entity;\n        private object _component;",
                "_entity = Core.Entity.Null;\n            _component = default!;",
                "var entry = _items.Current;\n                                _entity = entry.Item1;\n                                _component = entry.Item2;"
            )
        );

        sb.EndRegion();
    }

    private static void TableEntities(StringBuilder sb)
    {
        sb.BeginRegion("TableEntities");
        for (var i = 0; i < 16; i++)
            sb.AppendLine(TableQueryIterator("TableEntity", "Entity", "Entities", "_entity", i + 1, true));
        sb.EndRegion();
    }

    private static void TableComponents(StringBuilder sb)
    {
        sb.BeginRegion("TableComponents");
        for (var i = 0; i < 16; i++)
        {
            var type = i == 0 ? "object" : $"({string.Join(", ", Enumerable.Range(0, i + 1).Select(_ => "object"))})";
            var current =
                i == 0 ? "_field0" : $"({string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"))})";
            sb.AppendLine(TableQueryIterator("TableComponent", type, "Components", current, i + 1, noEntity: true));
        }

        sb.EndRegion();
    }

    private static void TableEntries(StringBuilder sb)
    {
        sb.BeginRegion("TableEntries");
        for (var i = 0; i < 15; i++)
        {
            var type =
                i == 0
                    ? "(Entity, object)"
                    : $"(Entity, {string.Join(", ", Enumerable.Range(0, i + 1).Select(_ => "object"))})";
            var getFields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var current = $"(_entity, {getFields})";
            sb.AppendLine(TableQueryIterator("TableEntry", type, "Entries", current, i + 1));
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
        bool noFields = false,
        bool noEntity = false,
        string refName = ""
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
                    
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool withDisabled = true) {
                        _withDisabled = withDisabled;
                        return ref this;
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} Deferred(bool deferred = true) {
                        _deferred = deferred;
                        return ref this;
                    }
                    {{(refName == "" ? "" : $$"""

                                    public {{refName}}Enumerable{{typeParams}} AsRef()
                                    {
                                        return new {{refName}}Enumerable{{typeParams}}(_scene).WithDisabled(_withDisabled).Deferred(_deferred);
                                    }
                            """
                        )}}
                }
                
                public unsafe struct {{name}}Enumerator{{typeParams}} : Collections.IStructEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", tables.Select((t, i) => $"        private Table<{t}> _table{i};"))}}
                    private int _index;
                    {{(tables.Count > 1 ? "private int _tableIndex; " : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _disposed;
            {{(noFields ? "" : string.Join("\n", tables.Select((t, i) => $"        private {t} _field{i} = default!;")))}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _disposed = true;
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
                                    {{(noEntity && tables.Count <= 1 ? "" : $"{(noEntity ? "var entity" : "_entity")} = new Entity(_table{i}.EntityIds[_index], _scene);")}}
                                    if (!_withDisabled && _scene.DisabledTable.Has({{(noEntity && tables.Count <= 1 ? $"new Entity(_table{i}.EntityIds[_index], _scene)" : noEntity ? "entity" : "_entity")}}))
                                        goto TABLE{{i}};
                {{string.Join("\n", tables.Select((_, j) => j == i ? "" : $"""
                                        ref var field{j} = ref _table{j}.GetRef({(noEntity ? "entity" : "_entity")}).Value;
                                        if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref field{j}))
                                            goto TABLE{i};
                                        {(noFields ? "" : $"_field{j} = field{j};")}
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
                        {{(noEntity ? "" : "_entity = Core.Entity.Null;")}}
            {{(noFields ? "" : string.Join("\n", tables.Select((_, i) => $"            _field{i} = default!;")))}}{{(tables.Count > 1 ? "\n            var smallestCount = int.MaxValue;\n" : "")}}
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
                
                public {{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}() {
                    ThrowIfNotInitialized();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }
                
            """;
    }

    private static string TableQueryIterator(
        string namePrefix,
        string type,
        string methodName,
        string current,
        int tableCount,
        bool noFields = false,
        bool noEntity = false
    )
    {
        return $$"""
                public struct {{namePrefix}}{{tableCount}}Enumerable : Collections.IStructEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>
                {
                    private readonly Scene _scene;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                    private bool _withDisabled;
                    private bool _deferred;
                
                    internal {{namePrefix}}{{tableCount}}Enumerable(Scene scene, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                    {
                        _scene = scene;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                        _deferred = true;
                    }
                    
                    public {{namePrefix}}{{tableCount}}Enumerator GetEnumerator()
                    {
                        return new {{namePrefix}}{{tableCount}}Enumerator(_scene, _withDisabled, _deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"_table{n}"))}});
                    }
                    
                    public ZLinq.ValueEnumerable<Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>, {{type}}> AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>(GetEnumerator());
                    }
                    
                    public ref {{namePrefix}}{{tableCount}}Enumerable WithDisabled(bool withDisabled = true) {
                        _withDisabled = withDisabled;
                        return ref this;
                    }
                    
                    public ref {{namePrefix}}{{tableCount}}Enumerable Deferred(bool deferred = true) {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public unsafe struct {{namePrefix}}{{tableCount}}Enumerator : Collections.IStructEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                    private int _index;
                    {{(tableCount > 1 ? "private int _tableIndex; " : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _disposed;
            {{(noFields ? "" : string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private object _field{n} = null!;")))}}

                    internal {{namePrefix}}{{tableCount}}Enumerator(Scene scene, bool withDisabled, bool deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _disposed = true;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                        Reset();
                    }

                    public bool MoveNext()
                    {
                        {{(tableCount > 1 ? "switch (_tableIndex)\n            " : "")}}{
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(i => $$"""
                                {{(tableCount > 1 ? $"case {i}:\n                " : "")}}{
                                    TABLE{{i}}:
                                    var newIndex = _index + 1;
                                    if (newIndex >= _table{{i}}.Count)
                                        return false;
                                    _index = newIndex;
                                    {{(noEntity && tableCount <= 1 ? "" : $"{(noEntity ? "var entity" : "_entity")} = new Entity(_table{i}.EntityIds[_index], _scene);")}}
                                    if (!_withDisabled && _scene.DisabledTable.Has({{(noEntity && tableCount <= 1 ? $"new Entity(_table{i}.EntityIds[_index], _scene)" : noEntity ? "entity" : "_entity")}}))
                                        goto TABLE{{i}};
                {{string.Join("\n", Enumerable.Range(0, tableCount).Where(j => j != i).Select(j => $$"""
                                        if (!_table{{j}}.TryGet({{(noEntity ? "entity" : "_entity")}}, out {{(noFields ? "_" : $"_field{j}")}}))
                                            goto TABLE{{i}};
                    """))}}
                                    {{(noFields ? "" : $"_field{i} = _table{i}.Get(_index);")}}
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
                        {{(noEntity ? "" : "_entity = Core.Entity.Null;")}}
            {{(noFields ? "" : string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _field{n} = default!;")))}}
            {{(tableCount > 1 ? "            var smallestCount = int.MaxValue;\n" : "")}}
            {{(tableCount > 1 ? string.Join("\n", Enumerable.Range(0, tableCount).Select(i => $$"""
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
                
                public {{namePrefix}}{{tableCount}}Enumerable {{methodName}}({{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}}) {
                    ThrowIfNotInitialized();
                    return new {{namePrefix}}{{tableCount}}Enumerable(this, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"table{n}"))}});
                }
                
            """;
    }

    private static string RefQueryIterator(
        string name,
        string type,
        string current,
        List<string> tables,
        string typeParams = "",
        bool noEntity = false
    )
    {
        return $$"""
                public struct {{name}}Enumerable{{typeParams}}
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
                    
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool withDisabled = true)
                    {
                        _withDisabled = withDisabled;
                        return ref this;
                    }

                    public ref {{name}}Enumerable{{typeParams}} Deferred(bool deferred = true)
                    {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public unsafe ref struct {{name}}Enumerator{{typeParams}}
                {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", tables.Select((t, i) => $"        private Table<{t}> _table{i};"))}}
                    private int _index;
                    {{(tables.Count > 1 ? "private int _tableIndex;" : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _disposed;
            {{string.Join("\n", tables.Select((t, i) => $"        private ComponentRef<{t}> _field{i};"))}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _disposed = true;
            {{string.Join("\n", tables.Select((t, i) => $"            _field{i} = ComponentRef<{t}>.Null;"))}}
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
                                    var entity = new Entity(_table{{i}}.EntityIds[_index], _scene);
                                    {{(noEntity ? "" : "_entity = entity;")}}
                                    if (!_withDisabled && _scene.DisabledTable.Has(entity))
                                        goto TABLE{{i}};
                {{string.Join("\n", tables.Select((_, j) => j == i ? "" : $"""
                                        _field{j} = _table{j}.GetRef(entity);
                                        if (_field{j}.IsNull)
                                            goto TABLE{i};
                    """).Where(str => str != ""))}}
                                    _field{{i}} = _table{{i}}.GetRef(_index);
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
                        {{(noEntity ? "" : "_entity = Core.Entity.Null;")}}
            {{string.Join("\n", tables.Select((t, i) => $"            _field{i} = ComponentRef<{t}>.Null;"))}}
            {{(tables.Count > 1 ? "\n            var smallestCount = int.MaxValue;\n" : "")}}
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
                
            """;
    }

    private static string AssignableQueryIterator(
        string name,
        string type,
        string methodName,
        string current,
        string typeParams,
        int typeCount,
        string iteratorType,
        string iteratorFieldName,
        string iteratorFactoryMethod,
        string stateFieldDeclarations,
        string stateResetStatements,
        string moveNextStateAssignments
    )
    {
        return $$"""
                public struct {{name}}Enumerable{{typeParams}} : Collections.IStructEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>
                {
                    private readonly Scene _scene;
                    private bool _withDisabled;
                    private bool _withHidden;
                    private bool _deferred;

                    internal {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                        _deferred = true;
                    }
                    
                    public {{name}}Enumerator{{typeParams}} GetEnumerator()
                    {
                        return new {{name}}Enumerator{{typeParams}}(_scene, _withDisabled, _withHidden, _deferred);
                    }
                    
                    public ZLinq.ValueEnumerable<Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>, {{type}}> AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>(GetEnumerator());
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool withDisabled = true)
                    {
                        _withDisabled = withDisabled;
                        return ref this;
                    }

                    public ref {{name}}Enumerable{{typeParams}} WithHidden(bool withHidden = true)
                    {
                        _withHidden = withHidden;
                        return ref this;
                    }
                    
                    public ref {{name}}Enumerable{{typeParams}} Deferred(bool deferred = true)
                    {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public struct {{name}}Enumerator{{typeParams}} : Collections.IStructEnumerator<{{type}}>
                {
                    private readonly Scene _scene;
                    private readonly bool _withDisabled;
                    private readonly bool _withHidden;
                    private readonly bool _deferred;
            {{string.Join("\n", Enumerable.Range(0, typeCount).Select(i => $"        private TableEnumerator<T{i}> _tables{i};"))}}
                    private {{iteratorType}} {{iteratorFieldName}};
                    private int _tableIndex;
                    private bool _hasIterator;
                    private bool _disposed;
                    {{stateFieldDeclarations}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool withHidden, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _withHidden = withHidden;
                        _deferred = deferred;
            {{string.Join("\n", Enumerable.Range(0, typeCount).Select(i => $"            _tables{i} = _scene.Tables<T{i}>().WithHidden(_withHidden).GetEnumerator();"))}}
                        {{iteratorFieldName}} = default;
                        _tableIndex = 0;
                        _hasIterator = false;
                        _disposed = true;
                        {{stateResetStatements}}
                        Reset();
                    }

                    public bool MoveNext()
                    {
                        while (true)
                        {
                            if (_hasIterator && {{iteratorFieldName}}.MoveNext())
                            {
                                {{moveNextStateAssignments}}
                                return true;
                            }

                            if (_hasIterator)
                            {
                                {{iteratorFieldName}}.Dispose();
                                _hasIterator = false;
                            }
                            if (!MoveNextTable())
                                return false;
                        }
                    }

                    private bool MoveNextTable()
                    {
                        while (true)
                        {
                            switch (_tableIndex)
                            {
            {{string.Join("\n", Enumerable.Range(0, typeCount).Select(i => $$"""
                                    case {{i}}:
                                        if (_tables{{i}}.MoveNext())
                                        {
                                            {{iteratorFieldName}} = _scene.{{iteratorFactoryMethod}}(_tables{{i}}.Current).WithDisabled(_withDisabled).Deferred(false).GetEnumerator();
                                            _hasIterator = true;
                                            return true;
                                        }

                                        _tables{{i}}.Dispose();
                                        _tableIndex++;
                                        continue;
                """))}}
                                default:
                                    return false;
                            }
                        }
                    }

                    public void Reset()
                    {
                        Dispose();
            {{string.Join("\n", Enumerable.Range(0, typeCount).Select(i => $"            _tables{i} = _scene.Tables<T{i}>().WithHidden(_withHidden).GetEnumerator();"))}}
                        {{iteratorFieldName}} = default;
                        _tableIndex = 0;
                        _hasIterator = false;
                        {{stateResetStatements}}
                        if (_deferred)
                            _scene.BeginDefer();
                        _disposed = false;
                    }

                    public {{type}} Current => {{current}};

                    public void Dispose()
                    {
                        if (_disposed)
                            return;
                        if (_hasIterator)
                        {
                            {{iteratorFieldName}}.Dispose();
                            _hasIterator = false;
                        }
            {{string.Join("\n", Enumerable.Range(0, typeCount).Select(i => $"            _tables{i}.Dispose();"))}}
                        if (_deferred)
                            _scene.EndDefer();
                        _disposed = true;
                    }
                }
                
                public {{name}}Enumerable{{typeParams}} {{methodName}}{{typeParams}}()
                {
                    ThrowIfNotInitialized();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }
                
            """;
    }
}

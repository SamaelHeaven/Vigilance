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
            namespace Vigilance.Core;

            public sealed partial class Scene
            {

            """
        );
        Create(sb);
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

    private static void Create(StringBuilder sb)
    {
        sb.BeginRegion("Create");
        for (var i = 0; i < 16; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var wheres = string.Join(" ", Enumerable.Range(0, i + 1).Select(n => $"where T{n} : IGameSystem, new()"));
            var newArgs = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"new T{n}()"));
            sb.AppendLine(
                $$"""
                    public static Scene Create<{{typeParams}}>(GameSystemsFunc? systems = null)
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
        sb.AppendLine(QueryIterator("Entity", "Entity", "Entities", "_entity", ["EntityTag"], noFields: true));
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
            var type = i == 0 ? "T0" : NamedTuple(false, Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList());
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
            var type = NamedTuple(true, Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList());
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
            var type = i == 0 ? "ComponentRef<T0>" : $"ComponentTuple<{typeParams}>";
            var fields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var current = i == 0 ? "_field0" : $"new ComponentTuple<{typeParams}>({fields})";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(
                RefQueryIterator("RefComponent", type, current, tables, "RefComponents", $"<{typeParams}>", true)
            );
        }

        sb.EndRegion();
    }

    private static void RefEntries(StringBuilder sb)
    {
        sb.BeginRegion("RefEntries");
        for (var i = 0; i < 15; i++)
        {
            var typeParams = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"T{n}"));
            var fields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var type = $"EntryTuple<{typeParams}>";
            var current = $"new EntryTuple<{typeParams}>(_entity, {fields})";
            var tables = Enumerable.Range(0, i + 1).Select(n => $"T{n}").ToList();
            sb.AppendLine(RefQueryIterator("RefEntry", type, current, tables, "RefEntries", $"<{typeParams}>"));
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
                "TableEntity1Enumerator",
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
                "TableComponent1Enumerator",
                "private object _component = default!;",
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
                "(Entity Entity, T0 Component)",
                "AssignableEntries",
                "(_entity, (T0)_component)",
                "TableEntry1Enumerator",
                "private Entity _entity;\n        private object _component = default!;",
                "_entity = Core.Entity.Null;\n            _component = default!;",
                "var entry = _items.Current;\n                    _entity = entry.Item1;\n                    _component = entry.Item2;"
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
            var type = i == 0 ? "object" : NamedTuple(false, Enumerable.Range(0, i + 1).Select(_ => "object").ToList());
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
            var type = NamedTuple(true, Enumerable.Range(0, i + 1).Select(_ => "object").ToList());
            var getFields = string.Join(", ", Enumerable.Range(0, i + 1).Select(n => $"_field{n}"));
            var current = $"(_entity, {getFields})";
            sb.AppendLine(TableQueryIterator("TableEntry", type, "Entries", current, i + 1));
        }

        sb.EndRegion();
    }

    private static string NamedTuple(bool hasEntity, IReadOnlyList<string> componentTypes)
    {
        return QueryHelper.NamedTuple(hasEntity, componentTypes);
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
        var isSingle = tables.Count == 1;
        const string singleCount = """
                        if (_withDisabled || _disabledTable.Count == 0)
                        {
                            count = _table0.Count;
                            return true;
                        }

                        count = 0;
                        return false;
            """;
        var tryCount = """
                        count = 0;
                        return false;
            """;
        var trySpan = """
                        span = default;
                        return false;
            """;
        var tryCopy = "            return false;";
        switch (isSingle)
        {
            case true when name == "Entity":
                tryCount = singleCount;
                tryCopy = """
                                if (_withDisabled || _disabledTable.Count == 0)
                                {
                                    var ids = _table0.EntityIds.AsSpan();
                                    if (ZLinq.Internal.EnumeratorHelper.TryGetSlice(ids, offset, destination.Length, out var slice))
                                    {
                                        for (var i = 0; i < slice.Length; i++)
                                            destination[i] = new Entity(slice[i], _scene);
                                        return true;
                                    }
                                }

                                return false;
                    """;
                break;
            case true when name == "Component":
                tryCount = singleCount;
                trySpan = """
                                if (_withDisabled || _disabledTable.Count == 0)
                                {
                                    span = _table0.Components.AsSpan();
                                    return true;
                                }

                                span = default;
                                return false;
                    """;
                tryCopy = """
                                if (_withDisabled || _disabledTable.Count == 0)
                                    return Collections.SpanExtensions.TryCopyTo(_table0.Components.AsSpan(), destination, offset);

                                return false;
                    """;
                break;
            case true when name == "Entry":
                tryCount = singleCount;
                tryCopy = """
                                if (_withDisabled || _disabledTable.Count == 0)
                                {
                                    var ids = _table0.EntityIds.AsSpan();
                                    var components = _table0.Components.AsSpan();
                                    if (ZLinq.Internal.EnumeratorHelper.TryGetSlice(ids, offset, destination.Length, out var idSlice)
                                        && ZLinq.Internal.EnumeratorHelper.TryGetSlice(components, offset, destination.Length, out var componentSlice))
                                    {
                                        for (var i = 0; i < idSlice.Length; i++)
                                            destination[i] = (new Entity(idSlice[i], _scene), componentSlice[i]);
                                        return true;
                                    }
                                }

                                return false;
                    """;
                break;
        }

        return $$"""
                public struct {{name}}Enumerable{{typeParams}} : Collections.IStructEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>
                {
                    private readonly Scene _scene;
                    private bool _withDisabled;
                    private bool _deferred;
                
                    public {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                        _deferred = true;
                    }

                    public {{name}}Enumerator{{typeParams}} GetEnumerator()
                    {
                        return new {{name}}Enumerator{{typeParams}}(_scene, _withDisabled, _deferred);
                    }

                    public ZLinq.ValueEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}> AsValueEnumerable()
                    {
                        return new ZLinq.ValueEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>(GetEnumerator());
                    }

                    ZLinq.ValueEnumerable<Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>, {{type}}> Collections.IStructEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}>.AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{name}}Enumerator{{typeParams}}, {{type}}>(GetEnumerator());
                    }

                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool withDisabled = true) {
                        _withDisabled = withDisabled;
                        return ref this;
                    }
                    
                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
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
                
                public struct {{name}}Enumerator{{typeParams}} : Collections.IStructEnumerator<{{type}}>, ZLinq.IValueEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", tables.Select((t, i) => $"        private readonly Table<{t}> _table{i};"))}}
                    private readonly Table<Disabled> _disabledTable;
                    private int _index;
                    {{(isSingle ? "private int _currentIndex; " : "")}}
                    {{(tables.Count > 1 ? "private int _tableIndex; " : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _initialized;
                    private bool _disposed;
            {{(noFields ? "" : string.Join("\n", tables.Select((t, i) => $"        private {t} _field{i} = default!;")))}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _initialized = false;
                        _disposed = true;
            {{string.Join("\n", tables.Select((t, i) => $"            _table{i} = _scene.Table<{t}>();"))}}
                        _disabledTable = _scene.DisabledTable;
                    }

                    private void Initialize()
                    {
                        _index = 0;
                        {{(noEntity ? "" : "_entity = Core.Entity.Null;")}}
            {{(noFields ? "" : string.Join("\n", tables.Select((_, i) => $"            _field{i} = default!;")))}}{{(tables.Count > 1 ? "\n            var smallestCount = int.MaxValue;\n" : "")}}
            {{(tables.Count > 1 ? string.Join("\n", tables.Select((_, i) => $$"""
                            if (_table{{i}}.Count < smallestCount)
                            {
                                smallestCount = _table{{i}}.Count;
                                _tableIndex = {{i}};
                            }
                """)) : "")}}
                        _initialized = true;
                        _disposed = false;
                        if (_deferred)
                            _scene.BeginDefer();
                    }

                    public bool MoveNext()
                    {
                        if (!_initialized)
                            Initialize();

                        {{(tables.Count > 1 ? "switch (_tableIndex)\n            " : "")}}{
            {{string.Join("\n", tables.Select((_, i) => $$"""
                                {{(tables.Count > 1 ? $"case {i}:\n                " : "")}}{
                                    TABLE{{i}}:
                                    if ((uint)_index >= (uint)_table{{i}}.Count)
                                    {
                                        _index = -1;
                                        return false;
                                    }

                                    var index = _index;
                                    _index++;
                                    {{(noEntity && tables.Count <= 1 ? "" : $"{(noEntity ? "var entity" : "_entity")} = new Entity(_table{i}.EntityIds[index], _scene);")}}
                                    if (!_withDisabled && _disabledTable.Has({{(noEntity && tables.Count <= 1 ? $"new Entity(_table{i}.EntityIds[index], _scene)" : noEntity ? "entity" : "_entity")}}))
                                        goto TABLE{{i}};
                {{string.Join("\n", tables.Select((_, j) => j == i ? "" : $"""
                                        ref var field{j} = ref _table{j}.GetRef({(noEntity ? "entity" : "_entity")}).Value;
                                        if (global::System.Runtime.CompilerServices.Unsafe.IsNullRef(ref field{j}))
                                            goto TABLE{i};
                                        {(noFields ? "" : $"_field{j} = field{j};")}
                    """).Where(str => str != ""))}}
                                    {{(noFields ? "" : $"_field{i} = _table{i}.Components[index];")}}
                                    {{(isSingle ? "_currentIndex = index;" : "")}}
                                    return true;
                                }
                """))}}
                        }

            #pragma warning disable CS0162
                        return false;
            #pragma warning restore CS0162
                    }

                    public bool TryGetNext(out {{type}} current)
                    {
                        global::System.Runtime.CompilerServices.Unsafe.SkipInit(out current);
                        if (MoveNext())
                        {
                            current = Current;
                            return true;
                        }

                        return false;
                    }

                    public bool TryGetNonEnumeratedCount(out int count)
                    {
            {{tryCount}}
                    }

                    public bool TryGetSpan(out global::System.ReadOnlySpan<{{type}}> span)
                    {
            {{trySpan}}
                    }

                    public bool TryCopyTo(scoped global::System.Span<{{type}}> destination, global::System.Index offset)
                    {
            {{tryCopy}}
                    }

                    public void Reset()
                    {
                        Dispose();
                        _initialized = false;
                    }

                    public {{type}} Current => {{current}};

                    {{(isSingle ? "public int CurrentIndex => _currentIndex;" : "")}}

                    public void Dispose()
                    {
                        if (_disposed)
                            return;
                        if (_deferred)
                            _scene.EndDefer();
                        _disposed = true;
                    }
                }

                public ZLinq.ValueEnumerable<{{name}}Enumerator{{typeParams}}, {{type}}> {{methodName}}{{typeParams}}(bool withDisabled = false, bool deferred = true) {
                    ThrowIfNotConfigured();
                    return new {{name}}Enumerable{{typeParams}}(this).WithDisabled(withDisabled).Deferred(deferred).AsValueEnumerable();
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
        var isSingle = tableCount == 1;
        var tryCount = """
                        count = 0;
                        return false;
            """;
        var tryCopy = "            return false;";
        if (!isSingle)
            return $$"""
                    public struct {{namePrefix}}{{tableCount}}Enumerable : Collections.IStructEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>
                    {
                        private readonly Scene _scene;
                {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                        private bool _withDisabled;
                        private bool _deferred;
                    
                        public {{namePrefix}}{{tableCount}}Enumerable(Scene scene, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                        {
                            _scene = scene;
                {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                            _deferred = true;
                        }
                        
                        public {{namePrefix}}{{tableCount}}Enumerator GetEnumerator()
                        {
                            return new {{namePrefix}}{{tableCount}}Enumerator(_scene, _withDisabled, _deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"_table{n}"))}});
                        }
                        
                        public ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}> AsValueEnumerable()
                        {
                            return new ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>(GetEnumerator());
                        }

                        ZLinq.ValueEnumerable<Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>, {{type}}> Collections.IStructEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>.AsValueEnumerable()
                        {
                            return new Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>(GetEnumerator());
                        }

                        [System.Diagnostics.CodeAnalysis.UnscopedRef]
                        public ref {{namePrefix}}{{tableCount}}Enumerable WithDisabled(bool withDisabled = true) {
                            _withDisabled = withDisabled;
                            return ref this;
                        }
                        
                        [System.Diagnostics.CodeAnalysis.UnscopedRef]
                        public ref {{namePrefix}}{{tableCount}}Enumerable Deferred(bool deferred = true) {
                            _deferred = deferred;
                            return ref this;
                        }
                    }
                    
                    public struct {{namePrefix}}{{tableCount}}Enumerator : Collections.IStructEnumerator<{{type}}>, ZLinq.IValueEnumerator<{{type}}> {
                        private readonly Scene _scene;
                        {{(noEntity ? "" : "private Entity _entity;")}}
                {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                        private readonly Table<Disabled> _disabledTable;
                        private int _index;
                        {{(tableCount == 1 ? "private int _currentIndex; " : "")}}
                        {{(tableCount > 1 ? "private int _tableIndex; " : "")}}
                        private readonly bool _withDisabled;
                        private readonly bool _deferred;
                        private bool _initialized;
                        private bool _disposed;
                {{(noFields ? "" : string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private object _field{n} = null!;")))}}

                        internal {{namePrefix}}{{tableCount}}Enumerator(Scene scene, bool withDisabled, bool deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                        {
                            _scene = scene;
                            _withDisabled = withDisabled;
                            _deferred = deferred;
                            _initialized = false;
                            _disposed = true;
                {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                            _disabledTable = _scene.DisabledTable;
                        }

                        private void Initialize()
                        {
                            _index = 0;
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
                            _initialized = true;
                            _disposed = false;
                            if (_deferred)
                                _scene.BeginDefer();
                        }

                        public bool MoveNext()
                        {
                            if (!_initialized)
                                Initialize();
                            {{(tableCount > 1 ? "switch (_tableIndex)\n            " : "")}}{
                {{string.Join("\n", Enumerable.Range(0, tableCount).Select(i => $$"""
                                    {{(tableCount > 1 ? $"case {i}:\n                " : "")}}{
                                        TABLE{{i}}:
                                        if ((uint)_index >= (uint)_table{{i}}.Count)
                                        {
                                            _index = -1;
                                            return false;
                                        }

                                        var index = _index;
                                        _index++;
                                        {{(noEntity && tableCount <= 1 ? "" : $"{(noEntity ? "var entity" : "_entity")} = new Entity(_table{i}.EntityIds[index], _scene);")}}
                                        if (!_withDisabled && _disabledTable.Has({{(noEntity && tableCount <= 1 ? $"new Entity(_table{i}.EntityIds[index], _scene)" : noEntity ? "entity" : "_entity")}}))
                                            goto TABLE{{i}};
                    {{string.Join("\n", Enumerable.Range(0, tableCount).Where(j => j != i).Select(j => $"""
                                            if (!_table{j}.TryGet({(noEntity ? "entity" : "_entity")}, out {(noFields ? "_" : $"_field{j}")}))
                                                goto TABLE{i};
                        """))}}
                                        {{(noFields ? "" : $"_field{i} = _table{i}.Get(index);")}}
                                        {{(tableCount == 1 ? "_currentIndex = index;" : "")}}
                                        return true;
                                    }
                    """))}}
                            }

                #pragma warning disable CS0162
                            return false;
                #pragma warning restore CS0162
                        }

                        public bool TryGetNext(out {{type}} current)
                        {
                            global::System.Runtime.CompilerServices.Unsafe.SkipInit(out current);
                            if (MoveNext())
                            {
                                current = Current;
                                return true;
                            }

                            return false;
                        }

                        public bool TryGetNonEnumeratedCount(out int count)
                        {
                {{tryCount}}
                        }

                        public bool TryGetSpan(out global::System.ReadOnlySpan<{{type}}> span)
                        {
                            span = default;
                            return false;
                        }

                        public bool TryCopyTo(scoped global::System.Span<{{type}}> destination, global::System.Index offset)
                        {
                {{tryCopy}}
                        }

                        public void Reset()
                        {
                            Dispose();
                            _initialized = false;
                        }

                        public {{type}} Current => {{current}};

                        {{(tableCount == 1 ? "public int CurrentIndex => _currentIndex;" : "")}}

                        public void Dispose()
                        {
                            if (_disposed)
                                return;
                            if (_deferred)
                                _scene.EndDefer();
                            _disposed = true;
                        }
                    }

                    public ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}> {{methodName}}({{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}}, bool withDisabled = false, bool deferred = true) {
                        ThrowIfNotConfigured();
                        return new {{namePrefix}}{{tableCount}}Enumerable(this, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"table{n}"))}}).WithDisabled(withDisabled).Deferred(deferred).AsValueEnumerable();
                    }

                """;
        tryCount = """
                        if (_withDisabled || _disabledTable.Count == 0)
                        {
                            count = _table0.Count;
                            return true;
                        }

                        count = 0;
                        return false;
            """;
        var assign = namePrefix switch
        {
            "TableEntity" => "destination[i] = new Entity(slice[i], _scene);",
            "TableComponent" => "destination[i] = _table0.Get(start + i);",
            _ => "destination[i] = (new Entity(slice[i], _scene), _table0.Get(start + i));",
        };
        tryCopy = $$"""
                        if (_withDisabled || _disabledTable.Count == 0)
                        {
                            var ids = _table0.EntityIds.AsSpan();
                            if (ZLinq.Internal.EnumeratorHelper.TryGetSlice(ids, offset, destination.Length, out var slice))
                            {
                                var start = offset.GetOffset(ids.Length);
                                for (var i = 0; i < slice.Length; i++)
                                    {{assign}}
                                return true;
                            }
                        }

                        return false;
            """;

        return $$"""
                public struct {{namePrefix}}{{tableCount}}Enumerable : Collections.IStructEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>
                {
                    private readonly Scene _scene;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                    private bool _withDisabled;
                    private bool _deferred;
                
                    public {{namePrefix}}{{tableCount}}Enumerable(Scene scene, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                    {
                        _scene = scene;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                        _deferred = true;
                    }
                    
                    public {{namePrefix}}{{tableCount}}Enumerator GetEnumerator()
                    {
                        return new {{namePrefix}}{{tableCount}}Enumerator(_scene, _withDisabled, _deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"_table{n}"))}});
                    }
                    
                    public ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}> AsValueEnumerable()
                    {
                        return new ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>(GetEnumerator());
                    }

                    ZLinq.ValueEnumerable<Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>, {{type}}> Collections.IStructEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>.AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{namePrefix}}{{tableCount}}Enumerator, {{type}}>(GetEnumerator());
                    }

                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{namePrefix}}{{tableCount}}Enumerable WithDisabled(bool withDisabled = true) {
                        _withDisabled = withDisabled;
                        return ref this;
                    }
                    
                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{namePrefix}}{{tableCount}}Enumerable Deferred(bool deferred = true) {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public struct {{namePrefix}}{{tableCount}}Enumerator : Collections.IStructEnumerator<{{type}}>, ZLinq.IValueEnumerator<{{type}}> {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private readonly Table _table{n};"))}}
                    private readonly Table<Disabled> _disabledTable;
                    private int _index;
                    {{(tableCount == 1 ? "private int _currentIndex; " : "")}}
                    {{(tableCount > 1 ? "private int _tableIndex; " : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _initialized;
                    private bool _disposed;
            {{(noFields ? "" : string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"        private object _field{n} = null!;")))}}

                    internal {{namePrefix}}{{tableCount}}Enumerator(Scene scene, bool withDisabled, bool deferred, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}})
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _initialized = false;
                        _disposed = true;
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(n => $"            _table{n} = table{n};"))}}
                        _disabledTable = _scene.DisabledTable;
                    }

                    private void Initialize()
                    {
                        _index = 0;
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
                        _initialized = true;
                        _disposed = false;
                        if (_deferred)
                            _scene.BeginDefer();
                    }

                    public bool MoveNext()
                    {
                        if (!_initialized)
                            Initialize();
                        {{(tableCount > 1 ? "switch (_tableIndex)\n            " : "")}}{
            {{string.Join("\n", Enumerable.Range(0, tableCount).Select(i => $$"""
                                {{(tableCount > 1 ? $"case {i}:\n                " : "")}}{
                                    TABLE{{i}}:
                                    if ((uint)_index >= (uint)_table{{i}}.Count)
                                    {
                                        _index = -1;
                                        return false;
                                    }

                                    var index = _index;
                                    _index++;
                                    {{(noEntity && tableCount <= 1 ? "" : $"{(noEntity ? "var entity" : "_entity")} = new Entity(_table{i}.EntityIds[index], _scene);")}}
                                    if (!_withDisabled && _disabledTable.Has({{(noEntity && tableCount <= 1 ? $"new Entity(_table{i}.EntityIds[index], _scene)" : noEntity ? "entity" : "_entity")}}))
                                        goto TABLE{{i}};
                {{string.Join("\n", Enumerable.Range(0, tableCount).Where(j => j != i).Select(j => $"""
                                        if (!_table{j}.TryGet({(noEntity ? "entity" : "_entity")}, out {(noFields ? "_" : $"_field{j}")}))
                                            goto TABLE{i};
                    """))}}
                                    {{(noFields ? "" : $"_field{i} = _table{i}.Get(index);")}}
                                    {{(tableCount == 1 ? "_currentIndex = index;" : "")}}
                                    return true;
                                }
                """))}}
                        }

            #pragma warning disable CS0162
                        return false;
            #pragma warning restore CS0162
                    }

                    public bool TryGetNext(out {{type}} current)
                    {
                        global::System.Runtime.CompilerServices.Unsafe.SkipInit(out current);
                        if (MoveNext())
                        {
                            current = Current;
                            return true;
                        }

                        return false;
                    }

                    public bool TryGetNonEnumeratedCount(out int count)
                    {
            {{tryCount}}
                    }

                    public bool TryGetSpan(out global::System.ReadOnlySpan<{{type}}> span)
                    {
                        span = default;
                        return false;
                    }

                    public bool TryCopyTo(scoped global::System.Span<{{type}}> destination, global::System.Index offset)
                    {
            {{tryCopy}}
                    }

                    public void Reset()
                    {
                        Dispose();
                        _initialized = false;
                    }

                    public {{type}} Current => {{current}};

                    {{(tableCount == 1 ? "public int CurrentIndex => _currentIndex;" : "")}}

                    public void Dispose()
                    {
                        if (_disposed)
                            return;
                        if (_deferred)
                            _scene.EndDefer();
                        _disposed = true;
                    }
                }

                public ZLinq.ValueEnumerable<{{namePrefix}}{{tableCount}}Enumerator, {{type}}> {{methodName}}({{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"Table table{n}"))}}, bool withDisabled = false, bool deferred = true) {
                    ThrowIfNotConfigured();
                    return new {{namePrefix}}{{tableCount}}Enumerable(this, {{string.Join(", ", Enumerable.Range(0, tableCount).Select(n => $"table{n}"))}}).WithDisabled(withDisabled).Deferred(deferred).AsValueEnumerable();
                }

            """;
    }

    private static string RefQueryIterator(
        string name,
        string type,
        string current,
        List<string> tables,
        string methodName,
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
                    
                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable{{typeParams}} WithDisabled(bool withDisabled = true)
                    {
                        _withDisabled = withDisabled;
                        return ref this;
                    }

                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable{{typeParams}} Deferred(bool deferred = true)
                    {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public ref struct {{name}}Enumerator{{typeParams}}
                {
                    private readonly Scene _scene;
                    {{(noEntity ? "" : "private Entity _entity;")}}
            {{string.Join("\n", tables.Select((t, i) => $"        private readonly Table<{t}> _table{i};"))}}
                    private readonly Table<Disabled> _disabledTable;
                    private int _index;
                    {{(tables.Count > 1 ? "private int _tableIndex;" : "")}}
                    private readonly bool _withDisabled;
                    private readonly bool _deferred;
                    private bool _initialized;
                    private bool _disposed;
            {{string.Join("\n", tables.Select((t, i) => $"        private ComponentRef<{t}> _field{i};"))}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _deferred = deferred;
                        _initialized = false;
                        _disposed = true;
            {{string.Join("\n", tables.Select((t, i) => $"            _field{i} = ComponentRef<{t}>.Null;"))}}
            {{string.Join("\n", tables.Select((t, i) => $"            _table{i} = _scene.Table<{t}>();"))}}
                        _disabledTable = _scene.DisabledTable;
                    }

                    private void Initialize()
                    {
                        _index = 0;
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
                        _initialized = true;
                        _disposed = false;
                        if (_deferred)
                            _scene.BeginDefer();
                    }

                    public bool MoveNext()
                    {
                        if (!_initialized)
                            Initialize();
                        {{(tables.Count > 1 ? "switch (_tableIndex)\n            " : "")}}{
            {{string.Join("\n", tables.Select((_, i) => $$"""
                                {{(tables.Count > 1 ? $"case {i}:\n                " : "")}}{
                                    TABLE{{i}}:
                                    if ((uint)_index >= (uint)_table{{i}}.Count)
                                    {    
                                        _index = -1;
                                        return false;
                                    }
                                        
                                    var index = _index;
                                    _index++;
                                    var entity = new Entity(_table{{i}}.EntityIds[index], _scene);
                                    {{(noEntity ? "" : "_entity = entity;")}}
                                    if (!_withDisabled && _disabledTable.Has(entity))
                                        goto TABLE{{i}};
                {{string.Join("\n", tables.Select((_, j) => j == i ? "" : $"""
                                        _field{j} = _table{j}.GetRef(entity);
                                        if (_field{j}.IsNull)
                                            goto TABLE{i};
                    """).Where(str => str != ""))}}
                                    _field{{i}} = _table{{i}}.GetRef(index);
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
                        _initialized = false;
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
                    ThrowIfNotConfigured();
                    return new {{name}}Enumerable{{typeParams}}(this);
                }

            """;
    }

    private static string AssignableQueryIterator(
        string name,
        string type,
        string methodName,
        string current,
        string iteratorType,
        string stateFieldDeclarations,
        string stateResetStatements,
        string moveNextStateAssignments
    )
    {
        return $$"""
                public struct {{name}}Enumerable<T0> : Collections.IStructEnumerable<{{name}}Enumerator<T0>, {{type}}>
                {
                    private readonly Scene _scene;
                    private bool _withDisabled;
                    private bool _withHidden;
                    private bool _deferred;

                    public {{name}}Enumerable(Scene scene)
                    {
                        _scene = scene;
                        _deferred = true;
                    }

                    public {{name}}Enumerator<T0> GetEnumerator()
                    {
                        return new {{name}}Enumerator<T0>(_scene, _withDisabled, _withHidden, _deferred);
                    }
                    
                    public ZLinq.ValueEnumerable<{{name}}Enumerator<T0>, {{type}}> AsValueEnumerable()
                    {
                        return new ZLinq.ValueEnumerable<{{name}}Enumerator<T0>, {{type}}>(GetEnumerator());
                    }

                    ZLinq.ValueEnumerable<Collections.StructEnumerator<{{name}}Enumerator<T0>, {{type}}>, {{type}}> Collections.IStructEnumerable<{{name}}Enumerator<T0>, {{type}}>.AsValueEnumerable()
                    {
                        return new Collections.StructEnumerator<{{name}}Enumerator<T0>, {{type}}>(GetEnumerator());
                    }

                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable<T0> WithDisabled(bool withDisabled = true)
                    {
                        _withDisabled = withDisabled;
                        return ref this;
                    }

                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable<T0> WithHidden(bool withHidden = true)
                    {
                        _withHidden = withHidden;
                        return ref this;
                    }
                    
                    [System.Diagnostics.CodeAnalysis.UnscopedRef]
                    public ref {{name}}Enumerable<T0> Deferred(bool deferred = true)
                    {
                        _deferred = deferred;
                        return ref this;
                    }
                }
                
                public struct {{name}}Enumerator<T0> : Collections.IStructEnumerator<{{type}}>, ZLinq.IValueEnumerator<{{type}}>
                {
                    private readonly Scene _scene;
                    private readonly bool _withDisabled;
                    private readonly bool _withHidden;
                    private readonly bool _deferred;
                    private TableEnumerator<T0> _tables;
                    private {{iteratorType}} _items;
                    private bool _hasIterator;
                    private bool _initialized;
                    private bool _disposed;
                    {{stateFieldDeclarations}}

                    internal {{name}}Enumerator(Scene scene, bool withDisabled, bool withHidden, bool deferred)
                    {
                        _scene = scene;
                        _withDisabled = withDisabled;
                        _withHidden = withHidden;
                        _deferred = deferred;
                        _initialized = false;
                        _disposed = true;
                    }

                    private void Initialize()
                    {
                        _tables = new TableEnumerable<T0>(_scene).WithHidden(_withHidden).GetEnumerator();
                        _items = default;
                        _hasIterator = false;
                        {{stateResetStatements}}
                        _initialized = true;
                        _disposed = false;
                        if (_deferred)
                            _scene.BeginDefer();
                    }

                    public bool MoveNext()
                    {
                        if (!_initialized)
                            Initialize();
                        while (true)
                        {
                            if (_hasIterator && _items.MoveNext())
                            {
                                {{moveNextStateAssignments}}
                                return true;
                            }
                            if (_hasIterator)
                            {
                                _items.Dispose();
                                _hasIterator = false;
                            }
                            if (!MoveNextTable())
                                return false;
                        }
                    }

                    private bool MoveNextTable()
                    {
                        if (_tables.MoveNext())
                        {
                            _items = new {{iteratorType.Replace(
                                "Enumerator",
                                "Enumerable"
                            )}}(_scene, _tables.Current).WithDisabled(_withDisabled).Deferred(false).GetEnumerator();
                            _hasIterator = true;
                            return true;
                        }
                        _tables.Dispose();
                        return false;
                    }

                    public bool TryGetNext(out {{type}} current)
                    {
                        global::System.Runtime.CompilerServices.Unsafe.SkipInit(out current);
                        if (MoveNext())
                        {
                            current = Current;
                            return true;
                        }

                        return false;
                    }

                    public bool TryGetNonEnumeratedCount(out int count)
                    {
                        count = 0;
                        return false;
                    }

                    public bool TryGetSpan(out global::System.ReadOnlySpan<{{type}}> span)
                    {
                        span = default;
                        return false;
                    }

                    public bool TryCopyTo(scoped global::System.Span<{{type}}> destination, global::System.Index offset)
                    {
                        return false;
                    }

                    public void Reset()
                    {
                        Dispose();
                        _initialized = false;
                    }

                    public {{type}} Current => {{current}};

                    public int CurrentIndex => _items.CurrentIndex;

                    public Table CurrentTable => _tables.Current;

                    public void Dispose()
                    {
                        if (_disposed)
                            return;
                        if (_hasIterator)
                        {
                            _items.Dispose();
                            _hasIterator = false;
                        }
                        _tables.Dispose();
                        if (_deferred)
                            _scene.EndDefer();
                        _disposed = true;
                    }
                }
                
                public ZLinq.ValueEnumerable<{{name}}Enumerator<T0>, {{type}}> {{methodName}}<T0>(bool withDisabled = false, bool withHidden = false, bool deferred = true)
                {
                    ThrowIfNotConfigured();
                    return new {{name}}Enumerable<T0>(this).WithDisabled(withDisabled).WithHidden(withHidden).Deferred(deferred).AsValueEnumerable();
                }

            """;
    }
}

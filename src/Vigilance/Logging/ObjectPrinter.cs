using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using ZLinq;
using ZLinq.Linq;

namespace Vigilance.Logging;

public static class ObjectPrinter
{
    public enum FilterType
    {
        Include,
        Exclude,
    }

    private static readonly Dictionary<Type, PropertyInfo[]> _properties = new();

    public static string Print<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        T obj,
        Filter? filter = null
    )
        where T : notnull
    {
        var type = typeof(T);
        var sb = new StringBuilder();
        sb.Append(type.Name);
        sb.Append(" { ");
        if (!_properties.TryGetValue(type, out var props))
        {
            props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            _properties[type] = props;
        }

        var i = 0;
        if (filter.HasValue)
            foreach (var prop in filter.Value.Apply(props))
                PrintProperty(sb, obj, prop, ref i);
        else
            foreach (var prop in props)
                PrintProperty(sb, obj, prop, ref i);
        if (i > 0)
            sb.Append(' ');
        sb.Append('}');
        return sb.ToString();
    }

    public static Filter Include(params string[] propertyNames)
    {
        return new Filter(FilterType.Include, propertyNames);
    }

    public static Filter Exclude(params string[] propertyNames)
    {
        return new Filter(FilterType.Exclude, propertyNames);
    }

    private static void PrintProperty(StringBuilder sb, object obj, PropertyInfo prop, ref int i)
    {
        if (i++ > 0)
            sb.Append(", ");
        var value = prop.GetValue(obj);
        sb.Append(prop.Name);
        sb.Append(" = ");
        sb.Append(value);
    }

    public readonly record struct Filter(FilterType Type, string[] PropertyNames)
    {
        public ValueEnumerable<ArrayWhere<PropertyInfo>, PropertyInfo> Apply(PropertyInfo[] properties)
        {
            var propertyNames = PropertyNames;
            return Type switch
            {
                FilterType.Include => properties.AsValueEnumerable().Where(p => propertyNames.Contains(p.Name)),
                FilterType.Exclude => properties.AsValueEnumerable().Where(p => !propertyNames.Contains(p.Name)),
                _ => throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(FilterType)),
            };
        }
    }
}

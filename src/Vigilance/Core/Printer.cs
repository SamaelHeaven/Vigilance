using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vigilance.Core;

public static class Printer
{
    public enum FilterType
    {
        Include,
        Exclude,
    }

    public static string Print<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
        T obj,
        Filter? filter = null
    )
        where T : notnull
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        var type = typeof(T);
        var sb = new StringBuilder();
        sb.Append(type.Name);
        sb.Append(" { ");
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var filteredProps = filter?.Apply(props) ?? props;
        var i = 0;
        foreach (var prop in filteredProps)
        {
            if (i++ > 0)
                sb.Append(", ");

            var value = prop.GetValue(obj);
            sb.Append(prop.Name);
            sb.Append(" = ");
            sb.Append(value is null ? "null" : value.ToString());
        }

        if (i > 0)
            sb.Append(' ');
        sb.Append('}');
        return sb.ToString();
    }

    public static Filter Include(params IEnumerable<string> propertyNames)
    {
        return new Filter(FilterType.Include, propertyNames);
    }

    public static Filter Exclude(params IEnumerable<string> propertyNames)
    {
        return new Filter(FilterType.Exclude, propertyNames);
    }

    public readonly record struct Filter(FilterType Type, IEnumerable<string> PropertyNames)
    {
        public IEnumerable<PropertyInfo> Apply(IEnumerable<PropertyInfo> properties)
        {
            var propertyNames = PropertyNames;
            return Type switch
            {
                FilterType.Include => properties.Where(p => propertyNames.Contains(p.Name)),
                FilterType.Exclude => properties.Where(p => !propertyNames.Contains(p.Name)),
                _ => properties,
            };
        }
    }
}

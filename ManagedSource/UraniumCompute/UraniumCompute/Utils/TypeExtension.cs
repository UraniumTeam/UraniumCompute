namespace UraniumCompute.Utils;

public static class TypeExtension
{
    private static readonly Dictionary<string, string> CSharpTypeAliases = new()
    {
        [nameof(Byte)] = "byte",
        [nameof(SByte)] = "sbyte",
        [nameof(Int16)] = "short",
        [nameof(UInt16)] = "ushort",
        [nameof(Int32)] = "int",
        [nameof(UInt32)] = "uint",
        [nameof(Int64)] = "long",
        [nameof(UInt64)] = "ulong",
        [nameof(Single)] = "float",
        [nameof(Double)] = "double",
        [nameof(Decimal)] = "decimal",
        [nameof(String)] = "string"
    };

    /// <summary>
    ///     Get human-readable type name.
    /// </summary>
    /// <param name="type">Type to get the name of.</param>
    /// <param name="addNamespace">True if the namespace needs to be added to the result.</param>
    /// <returns>The name of the type.</returns>
    public static string GetCSharpName(this Type type, bool addNamespace = false)
    {
        if (!type.IsGenericType)
        {
            return GetNonGenericTypeName(type, addNamespace);
        }

        var name = type.Name;
        var generics = string.Join(", ", type.GetGenericArguments().Select(t => t.GetCSharpName()));
        return AddNamespace(type, $"{name[..name.IndexOf('`')]}<{generics}>", addNamespace);
    }

    private static string AddNamespace(Type type, string name, bool addNamespace)
    {
        return addNamespace ? $"{type.Namespace}.{name}" : name;
    }

    private static string GetNonGenericTypeName(Type type, bool addNamespace)
    {
        return CSharpTypeAliases.TryGetValue(type.Name, out var alias)
            ? alias
            : AddNamespace(type, type.Name, addNamespace);
    }
}

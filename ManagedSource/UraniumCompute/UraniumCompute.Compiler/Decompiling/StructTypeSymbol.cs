using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class StructTypeSymbol : TypeSymbol
{
    public override string FullName { get; }
    public IEnumerable<FieldDesc> Fields => fields.Values;

    private readonly Dictionary<string, FieldDesc> fields;

    private StructTypeSymbol(string fullName, TypeReference typeReference, Func<string, string> fieldNameSelector,
        Action<TypeReference> userTypeCallback)
    {
        FullName = fullName;
        fields = new Dictionary<string, FieldDesc>();
        var type = typeReference.Resolve();
        foreach (var field in type.Fields.Where(x => x.IsPublic))
        {
            fields[field.FullName] = new FieldDesc(fieldNameSelector(field.Name),
                TypeResolver.CreateType(field.FieldType, userTypeCallback));
        }
    }

    public static StructTypeSymbol CreateSystemType(string fullName, TypeReference typeReference)
    {
        return new StructTypeSymbol(fullName, typeReference, x => x.ToLower(), _ => { });
    }

    public static StructTypeSymbol CreateUserType(TypeReference typeReference, Action<TypeReference> userTypeCallback)
    {
        return new StructTypeSymbol(MethodCompilation.DecorateName(typeReference.Name), typeReference,
            MethodCompilation.DecorateName, userTypeCallback);
    }

    public override bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc)
    {
        return fields.TryGetValue(field.FullName, out desc);
    }
}

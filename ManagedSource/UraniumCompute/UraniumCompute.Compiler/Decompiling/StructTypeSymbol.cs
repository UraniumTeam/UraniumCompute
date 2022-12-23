using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class StructTypeSymbol : TypeSymbol
{
    public override string FullName { get; }
    public IEnumerable<FieldDesc> Fields => fields.Values;
    public bool IsIntrinsicType { get; }

    private readonly Dictionary<string, FieldDesc> fields;

    private StructTypeSymbol(string fullName, TypeReference typeReference, Func<string, string> fieldNameSelector,
        bool isIntrinsicType, Action<TypeReference> userTypeCallback)
    {
        FullName = fullName;
        IsIntrinsicType = isIntrinsicType;
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
        return new StructTypeSymbol(fullName, typeReference, x => x.ToLower(), true, _ => { });
    }

    public static StructTypeSymbol CreateUserType(TypeReference typeReference, Action<TypeReference> userTypeCallback)
    {
        return new StructTypeSymbol(MethodCompilation.DecorateName(typeReference.Name), typeReference,
            MethodCompilation.DecorateName, false, userTypeCallback);
    }

    public override bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc)
    {
        return fields.TryGetValue(field.FullName, out desc);
    }
}

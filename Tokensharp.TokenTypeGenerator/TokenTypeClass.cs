using Microsoft.CodeAnalysis;

namespace Tokensharp.TokenTypeGenerator;

internal readonly record struct TokenTypeClass
{
    public readonly ITypeSymbol TypeSymbol;

    public TokenTypeClass(ITypeSymbol typeSymbol)
    {
        TypeSymbol = typeSymbol;
    }
}
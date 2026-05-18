using Microsoft.CodeAnalysis;

namespace Tokensharp.TokenTypeGenerator;

internal readonly record struct TokenTypeClass
{
    public readonly ITypeSymbol TypeSymbol;
    public readonly bool NumbersAreText;

    public TokenTypeClass(ITypeSymbol typeSymbol, bool numbersAreText)
    {
        TypeSymbol = typeSymbol;
        NumbersAreText = numbersAreText;
    }
}